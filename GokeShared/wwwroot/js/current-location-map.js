import { bindMarkerPopup, bindMarkerTooltip as bindCoreMarkerTooltip, createColoredIcon, createMap, normalizeMarkers } from "./map-core";

const resolveConfig = () => window.currentLocationMapConfig ?? {};

const currentLocationState = {
    mapHost: null,
    markers: new Map(),
    options: {},
    hasFittedBounds: false
};

const getEmptyStateElement = () => document.getElementById("empty-state");

const setEmptyState = (visible, message) => {
    const emptyStateElement = getEmptyStateElement();
    if (!emptyStateElement) {
        return;
    }

    emptyStateElement.textContent = message ?? "Current location is unavailable.";
    emptyStateElement.style.display = visible ? "flex" : "none";
};

const ensureMap = (options = {}) => {
    currentLocationState.options = {
        ...currentLocationState.options,
        ...options
    };

    if (currentLocationState.mapHost) {
        return currentLocationState.mapHost;
    }

    currentLocationState.mapHost = createMap({
        mapElementId: currentLocationState.options.mapElementId ?? "map",
        tileWarningMessage: currentLocationState.options.tileWarningMessage ?? "Map tiles are unavailable right now. Location markers are still shown."
    });

    return currentLocationState.mapHost;
};

const bindMarkerTooltip = (marker, item) => {
    bindCoreMarkerTooltip(marker, item);

    if (!item.name) {
        currentLocationState.mapHost?.unregisterTooltipMarker(marker);
        return;
    }

    currentLocationState.mapHost?.registerTooltipMarker(marker);
};

const updateMarkerAppearance = (marker, item) => {
    marker.setLatLng(item.point);
    marker.setOpacity(item.markerOpacity ?? 1);
    marker.setIcon(createColoredIcon(item.markerColor, item.markerShape, item.markerInitials));

    bindMarkerTooltip(marker, item);
    bindMarkerPopup(marker, item);
};

const shouldFitBounds = (validMarkers, options = {}) => {
    if (validMarkers.length === 0) {
        return false;
    }

    if (options.fitBounds === true) {
        return true;
    }

    if (options.fitBounds === false) {
        return false;
    }

    return !currentLocationState.hasFittedBounds;
};

export const initializeMap = (options = {}) => {
    ensureMap(options);
    setEmptyState(false, options.emptyMessage);

    return {
        mapHost: currentLocationState.mapHost,
        markers: Array.from(currentLocationState.markers.values())
    };
};

export const renderMarkers = (markers, options = {}) => {
    const mapHost = ensureMap(options);
    const validMarkers = normalizeMarkers(markers);
    const nextKeys = new Set(validMarkers.map((marker) => marker.key));

    for (const [key, marker] of currentLocationState.markers.entries()) {
        if (nextKeys.has(key)) {
            continue;
        }

        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
        currentLocationState.markers.delete(key);
    }

    for (const item of validMarkers) {
        let marker = currentLocationState.markers.get(item.key);

        if (!marker) {
            marker = L.marker(item.point, {
                icon: createColoredIcon(item.markerColor, item.markerShape, item.markerInitials)
            }).addTo(mapHost.map);

            currentLocationState.markers.set(item.key, marker);
        }

        updateMarkerAppearance(marker, item);
    }

    setEmptyState(validMarkers.length === 0, options.emptyMessage);

    if (shouldFitBounds(validMarkers, options)) {
        mapHost.fitBounds(
            validMarkers.map((marker) => marker.point),
            options.singlePointZoom ?? 15,
            options.padding ?? [30, 30]
        );

        currentLocationState.hasFittedBounds = true;
    }

    return {
        mapHost,
        markers: Array.from(currentLocationState.markers.values())
    };
};

export const clearMarkers = (options = {}) => {
    const mapHost = ensureMap(options);

    for (const marker of currentLocationState.markers.values()) {
        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
    }

    currentLocationState.markers.clear();
    setEmptyState(true, options.emptyMessage);
};

export const setMarkersJson = (markersJson, options = {}) => {
    const markers = typeof markersJson === "string"
        ? JSON.parse(markersJson)
        : markersJson;

    return renderMarkers(markers, options);
};

const renderAsync = async () => {
    const config = resolveConfig();

    initializeMap(config);

    renderMarkers(config.markers ?? [], {
        mapElementId: config.mapElementId,
        emptyMessage: config.emptyMessage,
        tileWarningMessage: config.tileWarningMessage,
        fitBounds: config.fitBounds,
        singlePointZoom: config.singlePointZoom,
        padding: config.padding
    });
};

export { renderAsync };
