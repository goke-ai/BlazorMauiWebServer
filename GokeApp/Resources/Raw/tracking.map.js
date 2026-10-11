import { createMap } from "__MAP_CORE_MODULE_URL__";

const resolveConfig = () => window.trackingMapConfig ?? {};
const trackingState = {
    mapHost: null,
    markers: new Map(),
    options: {},
    hasFittedBounds: false
};

const normalizePoint = (value) => {
    if (Array.isArray(value) && value.length >= 2) {
        return [Number(value[0]), Number(value[1])];
    }

    if (value && typeof value === "object") {
        const latitude = Number(value.latitude ?? value.lat);
        const longitude = Number(value.longitude ?? value.lng ?? value.lon);

        if (Number.isFinite(latitude) && Number.isFinite(longitude)) {
            return [latitude, longitude];
        }
    }

    return null;
};

const escapeHtml = (value) => String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");

const getShapeStyle = (shape) => {
    switch (shape) {
        case "Square":
            return "border-radius:3px;";
        case "Diamond":
            return "border-radius:3px;transform:rotate(45deg);";
        case "Triangle":
            return "width:0;height:0;border-left:9px solid transparent;border-right:9px solid transparent;border-bottom:18px solid currentColor;background:transparent;border-radius:0;box-shadow:none;color:";
        case "Hexagon":
            return "clip-path:polygon(25% 0%,75% 0%,100% 50%,75% 100%,25% 100%,0% 50%);";
        default:
            return "border-radius:50%;";
    }
};

const createColoredIcon = (markerColor, markerShape, markerInitials = "") => {
    const color = markerColor ?? "#0d6efd";
    const shape = markerShape ?? "Circle";
    const initials = String(markerInitials ?? "").slice(0, 3);
    const shapeStyle = getShapeStyle(shape);
    const label = initials
        ? `<span style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:8px;font-weight:700;color:#fff;transform:${shape === "Diamond" ? "rotate(-45deg)" : "none"};">${escapeHtml(initials)}</span>`
        : "";

    const html = shape === "Triangle"
        ? `<div style="position:relative;width:18px;height:18px;opacity:0.85;"><div style="${shapeStyle}${color};filter:drop-shadow(0 0 2px ${color}88);"></div><span style="position:absolute;left:50%;top:65%;transform:translate(-50%,-50%);font-size:7px;font-weight:700;color:#fff;">${escapeHtml(initials)}</span></div>`
        : `<div style="position:relative;width:14px;height:14px;opacity:0.85;${shapeStyle}background:${color};border:2px solid #fff;box-shadow:0 0 0 2px ${color}55;">${label}</div>`;

    return L.divIcon({
        className: "tracker-map-marker",
        html,
        iconSize: [18, 18],
        iconAnchor: [9, 9]
    });
};

const buildPopupHtml = (item) => {
    if (item.popupHtml) {
        return item.popupHtml;
    }

    const lines = [];

    if (item.name) {
        lines.push(`<strong>${escapeHtml(item.name)}</strong>`);
    }

    if (item.description) {
        lines.push(escapeHtml(item.description));
    }

    if (item.timestamp) {
        lines.push(`Time: ${escapeHtml(item.timestamp)}`);
    }

    return lines.join("<br/>");
};

const normalizeMarkers = (markers) => (markers ?? [])
    .map((marker, index) => ({
        ...marker,
        key: marker?.id != null && marker.id !== "" ? String(marker.id) : `__index_${index}`,
        point: normalizePoint(marker)
    }))
    .filter((marker) => Array.isArray(marker.point));

const getEmptyStateElement = () => document.getElementById("empty-state");

const setEmptyState = (visible, message) => {
    const emptyStateElement = getEmptyStateElement();
    if (!emptyStateElement) {
        return;
    }

    emptyStateElement.textContent = message ?? "No tracking markers available.";
    emptyStateElement.style.display = visible ? "flex" : "none";
};

const ensureMap = (options = {}) => {
    trackingState.options = {
        ...trackingState.options,
        ...options
    };

    if (trackingState.mapHost) {
        return trackingState.mapHost;
    }

    trackingState.mapHost = createMap({
        mapElementId: trackingState.options.mapElementId ?? "map",
        tileWarningMessage: trackingState.options.tileWarningMessage ?? "Map tiles are unavailable right now. Tracking markers are still shown."
    });

    return trackingState.mapHost;
};

const bindMarkerTooltip = (marker, item) => {
    const tooltipContent = item.name ? String(item.name) : "";
    const tooltip = marker.getTooltip?.();

    if (!tooltipContent) {
        if (tooltip) {
            marker.unbindTooltip();
        }

        trackingState.mapHost?.unregisterTooltipMarker(marker);

        return;
    }

    if (tooltip) {
        tooltip.setContent(tooltipContent);
        trackingState.mapHost?.registerTooltipMarker(marker);
        return;
    }

    marker.bindTooltip(tooltipContent, { direction: "top" });
    trackingState.mapHost?.registerTooltipMarker(marker);
};

const bindMarkerPopup = (marker, item) => {
    const popupHtml = buildPopupHtml(item);
    const popup = marker.getPopup?.();

    if (!popupHtml) {
        if (popup) {
            marker.unbindPopup();
        }

        return;
    }

    if (popup) {
        popup.setContent(popupHtml);
        return;
    }

    marker.bindPopup(popupHtml);
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

    return !trackingState.hasFittedBounds;
};

export const initializeMap = (options = {}) => {
    ensureMap(options);
    setEmptyState(false, options.emptyMessage);

    return {
        mapHost: trackingState.mapHost,
        markers: Array.from(trackingState.markers.values())
    };
};

export const renderMarkers = (markers, options = {}) => {
    const mapHost = ensureMap(options);
    const validMarkers = normalizeMarkers(markers);
    const nextKeys = new Set(validMarkers.map((marker) => marker.key));

    for (const [key, marker] of trackingState.markers.entries()) {
        if (nextKeys.has(key)) {
            continue;
        }

        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
        trackingState.markers.delete(key);
    }

    for (const item of validMarkers) {
        let marker = trackingState.markers.get(item.key);

        if (!marker) {
            marker = L.marker(item.point, {
                icon: createColoredIcon(item.markerColor, item.markerShape, item.markerInitials)
            }).addTo(mapHost.map);

            trackingState.markers.set(item.key, marker);
        }

        updateMarkerAppearance(marker, item);
    }

    setEmptyState(validMarkers.length === 0, options.emptyMessage);

    if (shouldFitBounds(validMarkers, options)) {
        mapHost.fitBounds(
            validMarkers.map((marker) => marker.point),
            options.singlePointZoom ?? 12,
            options.padding ?? [30, 30]
        );

        trackingState.hasFittedBounds = true;
    }

    return {
        mapHost,
        markers: Array.from(trackingState.markers.values())
    };
};

export const clearMarkers = (options = {}) => {
    const mapHost = ensureMap(options);

    for (const marker of trackingState.markers.values()) {
        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
    }

    trackingState.markers.clear();
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
