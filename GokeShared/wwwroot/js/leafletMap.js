const defaultLayerDefinitions = {
    default: {
        url: "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        options: {
            maxZoom: 18,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }
    },
    street: {
        url: "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png",
        options: {
            maxZoom: 20,
            subdomains: "abcd",
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
        }
    },
    terrain: {
        url: "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png",
        options: {
            maxZoom: 17,
            subdomains: "abc",
            attribution: 'Map data: &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, <a href="https://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a>'
        }
    },
    publicTransport: {
        url: "https://tile.memomaps.de/tilegen/{z}/{x}/{y}.png",
        options: {
            maxZoom: 18,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors | Public transport style &copy; <a href="https://memomaps.de">MeMoMaps</a>'
        }
    }
};

const mapInstances = new WeakMap();

function resolveElement(elementOrId) {
    if (typeof elementOrId === "string") {
        return document.getElementById(elementOrId);
    }

    return elementOrId ?? null;
}

function getMapInstance(elementOrId) {
    const element = resolveElement(elementOrId);
    return element ? mapInstances.get(element) ?? null : null;
}

function setMapInstance(element, map) {
    mapInstances.set(element, map);
}

function normalizePoint(value) {
    if (Array.isArray(value) && value.length >= 2) {
        return [Number(value[0]), Number(value[1])];
    }

    if (value && typeof value === "object") {
        return [Number(value.latitude), Number(value.longitude)];
    }

    return null;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

export class LeafletMap {
    constructor(elementOrId, latitudeOrOptions, longitude, layerId = "street") {
        if (!window.L) {
            throw new Error("Leaflet is not available on window.L.");
        }

        const element = resolveElement(elementOrId);
        if (!element) {
            throw new Error("Leaflet map container was not found.");
        }

        const options = latitudeOrOptions !== null
            && typeof latitudeOrOptions === "object"
            && !Array.isArray(latitudeOrOptions)
            ? latitudeOrOptions
            : {
                center: latitudeOrOptions !== undefined && longitude !== undefined
                    ? [latitudeOrOptions, longitude]
                    : null,
                layerId
            };

        this.element = element;
        this.layerDefinitions = options.layerDefinitions ?? defaultLayerDefinitions;
        this.defaultZoom = options.zoom ?? 15;
        this.defaultSinglePointZoom = options.singlePointZoom ?? 6;
        this.defaultFitMaxZoom = options.maxFitZoom ?? 16;
        this.layerId = null;
        this.tileLayer = null;
        this.singleMarker = null;
        this.multiMarkers = new Map();
        this.controls = [];

        this.map = L.map(this.element, {
            zoomControl: options.zoomControl ?? true,
            ...options.mapOptions
        });

        this.overlayLayer = L.layerGroup().addTo(this.map);
        this.setLayer(options.layerId ?? "street");

        if (options.center) {
            this.setView(options.center, options.zoom ?? this.defaultZoom);
        }

        setTimeout(() => this.map.invalidateSize(), 0);
        setMapInstance(this.element, this);
    }

    createTileLayer(layerId) {
        const definition = this.layerDefinitions[layerId] ?? this.layerDefinitions.street ?? this.layerDefinitions.default;
        return L.tileLayer(definition.url, definition.options);
    }

    setLayer(layerId = "street") {
        if (this.layerId === layerId && this.tileLayer) {
            return;
        }

        if (this.tileLayer) {
            this.map.removeLayer(this.tileLayer);
        }

        this.tileLayer = this.createTileLayer(layerId);
        this.tileLayer.addTo(this.map);
        this.layerId = layerId;
    }

    setView(center, zoom = this.defaultZoom) {
        const point = normalizePoint(center);
        if (!point) {
            return;
        }

        this.map.setView(point, zoom);
    }

    invalidateSize() {
        this.map.invalidateSize();
    }

    clearOverlays() {
        this.overlayLayer.clearLayers();
        this.singleMarker = null;
        this.multiMarkers.clear();
    }

    addLayer(layer) {
        this.overlayLayer.addLayer(layer);
        return layer;
    }

    addMarker(latLng, options = {}) {
        const marker = L.marker(latLng, options);
        return this.addLayer(marker);
    }

    addCircleMarker(latLng, options = {}) {
        const marker = L.circleMarker(latLng, options);
        return this.addLayer(marker);
    }

    createDefaultIcon() {
        return L.divIcon({
            className: "tracker-map-marker",
            html: '<div style="width:14px;height:14px;border-radius:50%;background:#dc3545;border:2px solid #fff;box-shadow:0 0 0 2px rgba(220,53,69,.35);"></div>',
            iconSize: [18, 18],
            iconAnchor: [9, 9]
        });
    }

    getShapeStyle(shape) {
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
    }

    createColoredIcon(color, shape, initials = "") {
        const shapeStyle = this.getShapeStyle(shape);
        const label = initials
            ? `<span style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:8px;font-weight:700;color:#fff;transform:${shape === "Diamond" ? "rotate(-45deg)" : "none"};">${escapeHtml(initials)}</span>`
            : "";

        const html = shape === "Triangle"
            ? `<div style="position:relative;width:18px;height:18px;"><div style="${shapeStyle}${color};filter:drop-shadow(0 0 2px ${color}88);"></div><span style="position:absolute;left:50%;top:65%;transform:translate(-50%,-50%);font-size:7px;font-weight:700;color:#fff;">${escapeHtml(initials)}</span></div>`
            : `<div style="position:relative;width:14px;height:14px;${shapeStyle}background:${color};border:2px solid #fff;box-shadow:0 0 0 2px ${color}55;">${label}</div>`;

        return L.divIcon({
            className: "tracker-map-marker",
            html,
            iconSize: [18, 18],
            iconAnchor: [9, 9]
        });
    }

    fitToPoints(points, options = {}) {
        const validPoints = (points ?? [])
            .map(normalizePoint)
            .filter(point => Array.isArray(point) && !Number.isNaN(point[0]) && !Number.isNaN(point[1]));

        if (validPoints.length === 0) {
            return;
        }

        if (validPoints.length === 1) {
            this.map.setView(validPoints[0], options.singlePointZoom ?? this.defaultSinglePointZoom);
            return;
        }

        this.map.fitBounds(validPoints, {
            padding: options.padding ?? [30, 30],
            maxZoom: options.maxZoom ?? this.defaultFitMaxZoom
        });
    }

    renderSingle(latitude, longitude, options = {}) {
        const point = normalizePoint([latitude, longitude]);
        if (!point) {
            return;
        }

        this.clearOverlays();
        this.singleMarker = this.addMarker(point, {
            icon: options.icon ?? this.createDefaultIcon()
        });

        if (options.tooltip) {
            this.singleMarker.bindTooltip(options.tooltip, { direction: "top" });
        }

        this.setView(point, options.zoom ?? this.defaultZoom);
    }

    updateSingle(latitude, longitude) {
        const point = normalizePoint([latitude, longitude]);
        if (!point) {
            return;
        }

        if (!this.singleMarker) {
            this.renderSingle(latitude, longitude);
            return;
        }

        this.singleMarker.setLatLng(point);
        this.setView(point, this.map.getZoom());
    }

    renderMulti(markers, options = {}) {
        if (!Array.isArray(markers) || markers.length === 0) {
            this.clearOverlays();
            return;
        }

        this.clearOverlays();

        for (const item of markers) {
            const point = normalizePoint(item);
            if (!point) {
                continue;
            }

            const marker = this.addMarker(point, {
                icon: this.createColoredIcon(
                    item.markerColor ?? "#0d6efd",
                    item.markerShape ?? "Circle",
                    item.markerInitials ?? ""
                )
            });

            marker.setOpacity(item.markerOpacity ?? 1);

            if (item.name) {
                marker.bindTooltip(item.name, { direction: "top" });
            }

            if (item.id !== undefined && item.id !== null) {
                this.multiMarkers.set(String(item.id), marker);
            }
        }

        this.fitToPoints(markers, {
            padding: options.padding ?? [30, 30],
            maxZoom: options.maxZoom ?? this.defaultFitMaxZoom
        });
    }

    updateMulti(markers, options = {}) {
        this.renderMulti(markers, options);
    }

    addControl(control) {
        this.map.addControl(control);
        this.controls.push(control);
        return control;
    }

    dispose() {
        for (const control of this.controls) {
            this.map.removeControl(control);
        }

        this.controls = [];
        this.clearOverlays();
        this.map.remove();
        mapInstances.delete(this.element);
    }
}

function getOrCreateMap(elementOrId, options = {}) {
    const existing = getMapInstance(elementOrId);
    if (existing) {
        if (options.layerId) {
            existing.setLayer(options.layerId);
        }

        return existing;
    }

    return new LeafletMap(elementOrId, options);
}

function replaceMap(elementOrId, options = {}) {
    dispose(elementOrId);
    return new LeafletMap(elementOrId, options);
}

export function render(elementOrId, latitude, longitude, layerId = "street") {
    const map = getOrCreateMap(elementOrId, {
        center: [latitude, longitude],
        layerId
    });

    map.renderSingle(latitude, longitude);
}

export function renderMulti(elementOrId, markers, layerId = "street") {
    const firstPoint = Array.isArray(markers) && markers.length > 0 ? normalizePoint(markers[0]) : null;
    const map = getOrCreateMap(elementOrId, {
        center: firstPoint ?? [0, 0],
        layerId
    });

    map.renderMulti(markers);
}

export function update(elementOrId, latitude, longitude) {
    const map = getOrCreateMap(elementOrId, {
        center: [latitude, longitude],
        layerId: "street"
    });

    map.updateSingle(latitude, longitude);
}

export function updateMulti(elementOrId, markers) {
    const map = getMapInstance(elementOrId);
    if (!map) {
        renderMulti(elementOrId, markers, "street");
        return;
    }

    map.updateMulti(markers);
}

export function setLayer(elementOrId, layerId) {
    const map = getMapInstance(elementOrId);
    if (!map) {
        return;
    }

    map.setLayer(layerId);
}

export function dispose(elementOrId) {
    const map = getMapInstance(elementOrId);
    if (!map) {
        return;
    }

    map.dispose();
}

export function renderWeatherMap(elementOrId, forecasts, scaleMinTemperature, scaleMaxTemperature) {
    const element = resolveElement(elementOrId);
    if (!element || !window.L) {
        return;
    }

    if (!Array.isArray(forecasts) || forecasts.length === 0) {
        dispose(element);
        return;
    }

    const map = replaceMap(element, {
        layerId: "default",
        zoom: 6,
        singlePointZoom: 6,
        maxFitZoom: 10
    });

    const minTemp = scaleMinTemperature ?? Math.min(...forecasts.map(forecast => forecast.temperatureC));
    const maxTemp = scaleMaxTemperature ?? Math.max(...forecasts.map(forecast => forecast.temperatureC));

    const getColor = (temp) => {
        if (minTemp === maxTemp) {
            return "hsl(20 85% 55%)";
        }

        const ratio = (temp - minTemp) / (maxTemp - minTemp);
        const hue = 220 - (220 * ratio);
        const lightness = 72 - (20 * ratio);

        return `hsl(${hue} 85% ${lightness}%)`;
    };

    const markers = [];
    let tooltipsVisible = false;
    let toggleButton = null;

    const setTooltipVisibility = (visible) => {
        tooltipsVisible = visible;

        for (const marker of markers) {
            if (visible) {
                marker.openTooltip();
            } else {
                marker.closeTooltip();
            }
        }

        if (toggleButton) {
            toggleButton.textContent = visible ? "Hide Temps" : "Show Temps";
            toggleButton.title = visible ? "Hide temperature labels" : "Show temperature labels";
            toggleButton.setAttribute("aria-pressed", visible ? "true" : "false");
        }
    };

    const TooltipToggleControl = L.Control.extend({
        options: {
            position: "topright"
        },
        onAdd: () => {
            const container = L.DomUtil.create("div", "leaflet-bar leaflet-control");
            toggleButton = L.DomUtil.create("a", "", container);

            toggleButton.href = "#";
            toggleButton.textContent = "Show Temps";
            toggleButton.title = "Show temperature labels";
            toggleButton.setAttribute("role", "button");
            toggleButton.setAttribute("aria-label", "Toggle temperature labels");
            toggleButton.setAttribute("aria-pressed", "false");
            toggleButton.style.width = "auto";
            toggleButton.style.padding = "0 10px";
            toggleButton.style.cursor = "pointer";

            L.DomEvent.disableClickPropagation(container);
            L.DomEvent.on(toggleButton, "click", (event) => {
                L.DomEvent.stop(event);
                setTooltipVisibility(!tooltipsVisible);
            });

            return container;
        }
    });

    map.addControl(new TooltipToggleControl());

    for (const forecast of forecasts) {
        const latLng = [forecast.latitude, forecast.longitude];
        const marker = map.addCircleMarker(latLng, {
            radius: 16,
            color: "#1f2937",
            weight: 2,
            fillColor: getColor(forecast.temperatureC),
            fillOpacity: 0.85
        });

        marker.bindTooltip(`${forecast.temperatureC}&deg;C`, {
            direction: "top",
            offset: [0, -14],
            opacity: 0.95,
            interactive: false
        });

        marker.bindPopup(
            `<strong>${escapeHtml(forecast.city)}</strong><br/>` +
            `Temperature: ${escapeHtml(forecast.temperatureC)}&deg;C<br/>` +
            `Summary: ${escapeHtml(forecast.summary)}<br/>` +
            `Time: ${escapeHtml(forecast.date)}`
        );

        markers.push(marker);
    }

    map.fitToPoints(
        forecasts.map(forecast => [forecast.latitude, forecast.longitude]),
        {
            padding: [30, 30],
            maxZoom: 10,
            singlePointZoom: 6
        }
    );

    setTooltipVisibility(false);
}