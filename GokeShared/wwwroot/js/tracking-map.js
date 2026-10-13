import { bindMarkerPopup, bindMarkerTooltip as bindCoreMarkerTooltip, createColoredIcon, createMap, normalizeMarkers } from "./map-core";

const resolveConfig = () => window.trackingMapConfig ?? {};

const trackingState = {
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
    bindCoreMarkerTooltip(marker, item);

    if (!item.name) {
        trackingState.mapHost?.unregisterTooltipMarker(marker);
        return;
    }

    trackingState.mapHost?.registerTooltipMarker(marker);
};

const updateMarkerAppearance = (marker, item) => {
    marker.setLatLng(item.point);
    marker.setOpacity(item.markerOpacity ?? 1);
    marker.setIcon(createColoredIcon(item.markerColor, item.markerShape, item.markerInitials));

    bindMarkerTooltip(marker, item);
    bindMarkerPopup(marker, item);
};

const shouldFitBounds = (validMarkers, markerCountIncreased, options = {}) => {
    if (validMarkers.length === 0) {
        return false;
    }

    if (options.fitBounds === true) {
        return true;
    }

    if (options.fitBounds === false) {
        return false;
    }

    return markerCountIncreased || !trackingState.hasFittedBounds;
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
    const previousMarkerCount = trackingState.markers.size;
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

    const markerCountIncreased = validMarkers.length > previousMarkerCount;

    if (shouldFitBounds(validMarkers, markerCountIncreased, options)) {
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
