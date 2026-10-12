import { bindMarkerPopup, bindMarkerTooltip as bindCoreMarkerTooltip, createColoredIcon, createMap, getColorByScale, getMinMaxWithOption, normalizeMarkers } from "__MAP_CORE_MODULE_URL__";

const resolveConfig = () => window.weatherMapConfig ?? {};

const weatherState = {
    mapHost: null,
    markers: new Map(),
    options: {},
    hasFittedBounds: false,
    initialScale: null
};

const getTemperatureScale = (markers, options = {}) => {
    if (weatherState.initialScale) {
        return weatherState.initialScale;
    }

    const values = markers.map((marker) => Number(marker.markerValue) || 0);

    const { min, max } = getMinMaxWithOption(values, {
        Min: options.scaleMinTemperature,
        Max: options.scaleMaxTemperature
    });

    const scale = {
        minTemp: min,
        maxTemp: max
    };

    weatherState.initialScale = scale;
    return scale;
};

const getColor = (value) => {
    const scale = weatherState.initialScale ?? getTemperatureScale(Array.from(weatherState.markers.values()), {});
    return getColorByScale(value, scale.minTemp, scale.maxTemp);
};

const getEmptyStateElement = () => document.getElementById("empty-state");

const setEmptyState = (visible, message) => {
    const emptyStateElement = getEmptyStateElement();
    if (!emptyStateElement) {
        return;
    }

    emptyStateElement.textContent = message ?? "Weather forecast is unavailable.";
    emptyStateElement.style.display = visible ? "flex" : "none";
};

const ensureMap = (options = {}) => {
    weatherState.options = {
        ...weatherState.options,
        ...options
    };

    if (weatherState.mapHost) {
        return weatherState.mapHost;
    }

    weatherState.mapHost = createMap({
        mapElementId: weatherState.options.mapElementId ?? "map",
        tileWarningMessage: weatherState.options.tileWarningMessage ?? "Map tiles are unavailable right now. Weather forecast markers are still shown."
    });

    return weatherState.mapHost;
};

const bindMarkerTooltip = (marker, item) => {
    bindCoreMarkerTooltip(marker, item);

    if (!item.name) {
        weatherState.mapHost?.unregisterTooltipMarker(marker);
        return;
    }

    weatherState.mapHost?.registerTooltipMarker(marker);
};

const updateMarkerAppearance = (marker, item) => {
    marker.setLatLng(item.point);
    marker.setOpacity(item.markerOpacity ?? 1);

    item.markerColor = getColor(item.markerValue);

    marker.setIcon(createColoredIcon(item.markerColor, item.markerShape, item.markerInitials, item.markerOpacity, 32, weatherState.mapHost?.map.getZoom()));

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

    return !weatherState.hasFittedBounds;
};

export const initializeMap = (options = {}) => {
    ensureMap(options);
    setEmptyState(false, options.emptyMessage);

    return {
        mapHost: weatherState.mapHost,
        markers: Array.from(weatherState.markers.values())
    };
};

export const renderMarkers = (markers, options = {}) => {
    const mapHost = ensureMap(options);
    const validMarkers = normalizeMarkers(markers);

    const nextKeys = new Set(validMarkers.map((marker) => marker.key));
    for (const [key, marker] of weatherState.markers.entries()) {
        if (nextKeys.has(key)) {
            continue;
        }

        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
        weatherState.markers.delete(key);
    }

    const scale = getTemperatureScale(validMarkers, {});

    for (const item of validMarkers) {
        let marker = weatherState.markers.get(item.key);

        if (!marker) {
            marker = L.marker(item.point, {
                icon: createColoredIcon(item.markerColor, item.markerShape, item.markerInitials)
            }).addTo(mapHost.map);

            weatherState.markers.set(item.key, marker);
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

        weatherState.hasFittedBounds = true;
    }

    return {
        mapHost,
        markers: Array.from(weatherState.markers.values())
    };
};

export const clearMarkers = (options = {}) => {
    const mapHost = ensureMap(options);

    for (const marker of weatherState.markers.values()) {
        mapHost.unregisterTooltipMarker(marker);
        mapHost.map.removeLayer(marker);
    }

    weatherState.markers.clear();
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
