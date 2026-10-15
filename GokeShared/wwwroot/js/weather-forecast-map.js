import { bindMarkerPopup, createMap, getColorByScale, getMarkerRadiusFromZoom, getMinMaxWithOption, normalizePoint, escapeHtml } from "./map-core.js";

const resolveConfig = () => window.weatherMapConfig ?? {};

const weatherState = {
    mapHost: null,
    markers: new Map(),
    options: {},
    initialScale: null
};

const getEmptyStateElement = () => document.getElementById("empty-state");

const setEmptyState = (visible, message) => {
    const emptyStateElement = getEmptyStateElement();
    if (!emptyStateElement) {
        return;
    }

    emptyStateElement.textContent = message ?? "No forecast data available.";
    emptyStateElement.style.display = visible ? "flex" : "none";
};

const updateMarkerSizes = () => {
    if (!weatherState.mapHost) {
        return;
    }

    //const radius = getMarkerRadiusFromZoom(weatherState.mapHost.map.getZoom());

    //for (const marker of weatherState.markers.values()) {
    //    marker.setRadius(radius);
    //}
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
        tileWarningMessage: weatherState.options.tileWarningMessage ?? "Map tiles are unavailable right now. Forecast markers are still shown."
    });

    //weatherState.mapHost.map.on("zoomend", updateMarkerSizes);

    return weatherState.mapHost;
};

const normalizeForecastsInput = (value) => {
    if (Array.isArray(value)) {
        return value;
    }

    if (typeof value === "string") {
        try {
            const parsed = JSON.parse(value);
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    }

    return [];
};

const normalizeForecasts = (forecasts) => normalizeForecastsInput(forecasts)
    .map((forecast, index) => {
        const point = normalizePoint(forecast);

        return {
            ...forecast,
            key: String(forecast?.city ?? `__index_${index}`) + `_${index}`,
            point,
            temperatureC: Number(forecast?.temperatureC)
        };
    })
    .filter((forecast) => Array.isArray(forecast.point) && Number.isFinite(forecast.temperatureC));

const getTemperatureScale = (forecasts, options = {}) => {
    if (weatherState.initialScale) {
        return weatherState.initialScale;
    }

    const values = forecasts.map((forecast) => Number(forecast.temperatureC) || 0);

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


const bindMarkerTooltip = (marker, forecast) => {
    const tooltipContent = `${forecast.temperatureC}&deg;C`;
    const tooltip = marker.getTooltip?.();

    if (tooltip) {
        tooltip.setContent(tooltipContent);
        return;
    }

    marker.bindTooltip(tooltipContent, {
        permanent: false,
        direction: "top",
        offset: [0, -14],
        opacity: 0.95
    });
};

const buildPopupData = (forecast) => ({
    popupHtml:
        `<strong>${escapeHtml(forecast.city)}</strong><br/>` +
        `Temperature: ${escapeHtml(forecast.temperatureC)}&deg;C<br/>` +
        `Summary: ${escapeHtml(forecast.summary)}<br/>` +
        `Time: ${escapeHtml(forecast.date)}`
});

const updateMarkerAppearance = (marker, forecast, scale) => {
    marker.setLatLng(forecast.point);
    marker.setStyle({
        color: "#1f2937",
        weight: 2,
        fillColor: getColorByScale(forecast.temperatureC, scale.minTemp, scale.maxTemp),
        fillOpacity: 0.85
    });

    bindMarkerTooltip(marker, forecast);
    bindMarkerPopup(marker, buildPopupData(forecast));
    weatherState.mapHost?.applyTooltipVisibility(marker);
};

const createMarker = (forecast, scale) => {
    const mapHost = ensureMap();
    const map = mapHost.map;
    const marker = L.circleMarker(forecast.point, {
        radius: 16,//getMarkerRadiusFromZoom(map.getZoom()),
        color: "#1f2937",
        weight: 2,
        fillColor: getColorByScale(forecast.temperatureC, scale.minTemp, scale.maxTemp),
        fillOpacity: 0.85 
    });

    bindMarkerTooltip(marker, forecast);
    bindMarkerPopup(marker, buildPopupData(forecast));
    marker.addTo(map);
    mapHost.registerTooltipMarker(marker);

    return marker;
};

const removeAllMarkers = () => {
    if (!weatherState.mapHost) {
        weatherState.markers.clear();
        return;
    }

    for (const marker of weatherState.markers.values()) {
        weatherState.mapHost.unregisterTooltipMarker(marker);
        weatherState.mapHost.map.removeLayer(marker);
    }

    weatherState.markers.clear();
};

const syncMarkers = (forecasts, scale) => {
    const activeKeys = new Set();

    for (const forecast of forecasts) {
        activeKeys.add(forecast.key);

        const existingMarker = weatherState.markers.get(forecast.key);
        if (existingMarker) {
            updateMarkerAppearance(existingMarker, forecast, scale);
            continue;
        }

        weatherState.markers.set(forecast.key, createMarker(forecast, scale));
    }

    for (const [key, marker] of weatherState.markers.entries()) {
        if (activeKeys.has(key)) {
            continue;
        }

        weatherState.mapHost.map.removeLayer(marker);
        weatherState.markers.delete(key);
    }
};

const fitForecastBounds = (forecasts) => {
    if (!weatherState.mapHost || forecasts.length === 0) {
        return;
    }

    weatherState.mapHost.fitBounds(
        forecasts.map((forecast) => forecast.point),
        6,
        [30, 30]);
};

export function clearForecasts(options = {}) {
    ensureMap(options);
    removeAllMarkers();
    setEmptyState(true, options.emptyMessage ?? resolveConfig().emptyMessage ?? "No forecast data available.");
}

export function setForecastsJson(forecastsJson, scaleMinTemperature, scaleMaxTemperature, options = {}) {
    ensureMap(options);

    const forecasts = normalizeForecasts(forecastsJson);
    if (forecasts.length === 0) {
        clearForecasts(options);
        return;
    }

    const scale = getTemperatureScale(forecasts, {
        scaleMinTemperature: Number(scaleMinTemperature),
        scaleMaxTemperature: Number(scaleMaxTemperature)
    });

    setEmptyState(false, options.emptyMessage ?? resolveConfig().emptyMessage ?? "No forecast data available.");
    syncMarkers(forecasts, scale);
    //updateMarkerSizes();
    fitForecastBounds(forecasts);
}

export async function renderAsync() {
    const config = resolveConfig();

    setForecastsJson(
        config.forecasts,
        config.scaleMinTemperature,
        config.scaleMaxTemperature,
        {
            emptyMessage: config.emptyMessage
        });
    return Promise.resolve();
}