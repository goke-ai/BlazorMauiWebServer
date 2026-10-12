import { bindMarkerPopup, bindMarkerTooltip, createColoredIcon, createMap, fitBounds } from "__MAP_CORE_MODULE_URL__";

const resolveConfig = () => window.directionMapConfig ?? {};
const directionState = {
    mapHost: null,
    routes: new Map(),
    options: {},
    hasFittedBounds: false
};

const getEmptyStateElement = () => document.getElementById("empty-state");

const setEmptyState = (visible, message) => {
    const emptyStateElement = getEmptyStateElement();
    if (!emptyStateElement) {
        return;
    }

    emptyStateElement.textContent = message ?? "No direction routes available.";
    emptyStateElement.style.display = visible ? "flex" : "none";
};

const normalizePoint = (latitude, longitude) => {
    const lat = Number(latitude);
    const lng = Number(longitude);
    return Number.isFinite(lat) && Number.isFinite(lng) ? [lat, lng] : null;
};

const normalizeRoutes = (routes) => (routes ?? [])
    .map((route, index) => {
        const startPoint = normalizePoint(route?.startLatitude, route?.startLongitude);
        const endPoint = normalizePoint(route?.endLatitude, route?.endLongitude);
        const currentPoint = normalizePoint(route?.currentLatitude, route?.currentLongitude);

        return {
            ...route,
            key: route?.id != null && route.id !== "" ? String(route.id) : `__index_${index}`,
            startPoint,
            endPoint,
            currentPoint
        };
    })
    .filter((route) => Array.isArray(route.startPoint) && Array.isArray(route.endPoint) && Array.isArray(route.currentPoint));

const ensureMap = (options = {}) => {
    directionState.options = {
        ...directionState.options,
        ...options
    };

    if (directionState.mapHost) {
        return directionState.mapHost;
    }

    directionState.mapHost = createMap({
        mapElementId: directionState.options.mapElementId ?? "map",
        tileWarningMessage: directionState.options.tileWarningMessage ?? "Map tiles are unavailable right now. Route markers are still shown."
    });

    return directionState.mapHost;
};

const createEndpointMarker = (point, color, label) => L.circleMarker(point, {
    radius: 5,
    weight: 2,
    color,
    fillColor: "#ffffff",
    fillOpacity: 1
}).bindTooltip(label, { direction: "top", opacity: 0.9 });

const updateRouteAppearance = (routeLayers, item, mapHost) => {
    routeLayers.line.setLatLngs([item.startPoint, item.endPoint]);
    routeLayers.line.setStyle({ color: item.markerColor ?? "#2563eb" });

    routeLayers.startMarker.setLatLng(item.startPoint);
    routeLayers.endMarker.setLatLng(item.endPoint);

    routeLayers.currentMarker.setLatLng(item.currentPoint);
    routeLayers.currentMarker.setIcon(createColoredIcon(item.markerColor, item.markerShape, item.markerInitials));

    bindMarkerTooltip(routeLayers.currentMarker, {
        name: `${item.name ?? "Route"} (${item.transportMode ?? "Move"})`
    });

    bindMarkerPopup(routeLayers.currentMarker, {
        name: item.name,
        description: item.description,
        timestamp: item.timestamp,
        popupHtml: item.popupHtml
    });

    mapHost.registerTooltipMarker(routeLayers.currentMarker);
};

const ensureRouteLayers = (item, mapHost) => {
    let routeLayers = directionState.routes.get(item.key);
    if (routeLayers) {
        return routeLayers;
    }

    routeLayers = {
        line: L.polyline([item.startPoint, item.endPoint], {
            color: item.markerColor ?? "#2563eb",
            weight: 4,
            opacity: 0.7,
            dashArray: item.transportMode === "Walk" ? "6 6" : null
        }).addTo(mapHost.map),
        startMarker: createEndpointMarker(item.startPoint, item.markerColor ?? "#2563eb", `${item.name ?? "Route"} start`).addTo(mapHost.map),
        endMarker: createEndpointMarker(item.endPoint, item.markerColor ?? "#2563eb", `${item.name ?? "Route"} end`).addTo(mapHost.map),
        currentMarker: L.marker(item.currentPoint, {
            icon: createColoredIcon(item.markerColor, item.markerShape, item.markerInitials)
        }).addTo(mapHost.map)
    };

    directionState.routes.set(item.key, routeLayers);
    return routeLayers;
};

export const initializeMap = (options = {}) => {
    ensureMap(options);
    setEmptyState(false, options.emptyMessage);

    return {
        mapHost: directionState.mapHost,
        routes: Array.from(directionState.routes.values())
    };
};

export const renderRoutes = (routes, options = {}) => {
    const mapHost = ensureMap(options);
    const validRoutes = normalizeRoutes(routes);
    const previousRouteCount = directionState.routes.size;
    const nextKeys = new Set(validRoutes.map((route) => route.key));

    for (const [key, routeLayers] of directionState.routes.entries()) {
        if (nextKeys.has(key)) {
            continue;
        }

        mapHost.unregisterTooltipMarker(routeLayers.currentMarker);
        mapHost.map.removeLayer(routeLayers.line);
        mapHost.map.removeLayer(routeLayers.startMarker);
        mapHost.map.removeLayer(routeLayers.endMarker);
        mapHost.map.removeLayer(routeLayers.currentMarker);
        directionState.routes.delete(key);
    }

    const bounds = [];

    for (const item of validRoutes) {
        const routeLayers = ensureRouteLayers(item, mapHost);
        updateRouteAppearance(routeLayers, item, mapHost);
        bounds.push(item.startPoint, item.endPoint, item.currentPoint);
    }

    setEmptyState(validRoutes.length === 0, options.emptyMessage);

    if (validRoutes.length > 0 && (validRoutes.length > previousRouteCount || !directionState.hasFittedBounds)) {
        fitBounds(mapHost.map, bounds, options.singlePointZoom ?? 12, options.padding ?? [36, 36]);
        directionState.hasFittedBounds = true;
    }

    return {
        mapHost,
        routes: Array.from(directionState.routes.values())
    };
};

export const clearRoutes = (options = {}) => {
    const mapHost = ensureMap(options);

    for (const routeLayers of directionState.routes.values()) {
        mapHost.unregisterTooltipMarker(routeLayers.currentMarker);
        mapHost.map.removeLayer(routeLayers.line);
        mapHost.map.removeLayer(routeLayers.startMarker);
        mapHost.map.removeLayer(routeLayers.endMarker);
        mapHost.map.removeLayer(routeLayers.currentMarker);
    }

    directionState.routes.clear();
    setEmptyState(true, options.emptyMessage);
};

export const setRoutesJson = (routesJson, options = {}) => {
    const routes = typeof routesJson === "string"
        ? JSON.parse(routesJson)
        : routesJson;

    return renderRoutes(routes, options);
};

const renderAsync = async () => {
    const config = resolveConfig();

    initializeMap(config);
    renderRoutes(config.routes ?? [], {
        mapElementId: config.mapElementId,
        emptyMessage: config.emptyMessage,
        tileWarningMessage: config.tileWarningMessage,
        singlePointZoom: config.singlePointZoom,
        padding: config.padding
    });
};

export { renderAsync };
