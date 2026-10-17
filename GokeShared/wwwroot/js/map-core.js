
export const toRGBAValue = (color, alpha = 1) => {
    if (typeof color !== 'string' || color.trim() === '') {
        console.error("Invalid color input.");
        return null;
    }

    const normalizedColor = color.trim();
    const style = new Option().style;
    style.color = "";
    style.color = normalizedColor;

    if (style.color === "") {
        console.error("Invalid CSS color format.");
        return null;
    }

    const tempElem = document.createElement("canvas");
    tempElem.width = 1;
    tempElem.height = 1;

    const ctx = tempElem.getContext("2d");

    if (!ctx) {
        console.error("Could not create canvas context.");
        return null;
    }

    ctx.clearRect(0, 0, 1, 1);
    ctx.fillStyle = style.color;
    ctx.fillRect(0, 0, 1, 1);

    const [r, g, b] = ctx.getImageData(0, 0, 1, 1).data;

    return { r, g, b, alpha };
};

export const toHexWithAlpha = (color, alpha = 1) => {
    const rgba = toRGBAValue(color, alpha);

    if (!rgba) {
        return null;
    }

    const { r, g, b, alpha: a } = rgba;

    // Convert to hex and pad with zeros if needed
    const hex = "#" +
        [r, g, b]
            .map(val => val.toString(16).padStart(2, "0"))
            .join("") +
        Math.round(a * 255).toString(16).padStart(2, "0");

    return hex.toLowerCase();
}

export const toRGBA = (color, alpha = 1) => {
    const rgba = toRGBAValue(color, alpha);

    if (!rgba) {
        return null;
    }

    const { r, g, b, alpha: a } = rgba;

    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
};

export const escapeHtml = (value) => String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");

export const getMinMaxOrOption = (values, options = {}) => {
    
    if (values.length === 0) {
        return {
            min: 0,
            max: 0
        };
    }

    const scale = {
        min: Number.isFinite(options.Min)
            ? options.Min
            : Math.min(...values.map((value) => Number(value) || 0)),
        max: Number.isFinite(options.Max)
            ? options.Max
            : Math.max(...values.map((value) => Number(value) || 0))
    };

    return scale;
};

export const getMinMaxWithOption = (values, options = {}) => {

    if (values.length === 0) {
        return {
            min: 0,
            max: 0
        };
    }

    const all = [Number.isFinite(options.Min) ? options.Min : 0, Number.isFinite(options.Max) ? options.Max : 0, ...values];
   
    return getMinMax(all);
};

export const getMinMax = (values) => {

    if (values.length === 0) {
        return {
            min: 0,
            max: 0
        };
    }

    const scale = {
        min: Math.min(...values.map((value) => Number(value) || 0)),
        max: Math.max(...values.map((value) => Number(value) || 0))
    };

    return scale;
};

export const normalizePoint = (value) => {
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

export const normalizeMarkers = (markers) => (markers ?? [])
    .map((marker, index) => ({
        ...marker,
        key: marker?.id != null && marker.id !== "" ? String(marker.id) : `__index_${index}`,
        point: normalizePoint(marker)
    }))
    .filter((marker) => Array.isArray(marker.point));


export const getMarkerRadiusFromZoom = (zoom) => {
    const normalizedZoom = Number.isFinite(zoom) ? zoom : 6;
    return Math.max(6, Math.min(14, Math.round(normalizedZoom * 1.1)));
};

export const getMarkerIconSizeFromZoom = (zoom) => Math.max(
    18,
    Math.min(32, Math.round(getMarkerRadiusFromZoom(zoom) * 2))
);

export const getColorByScale = (value, minValue, maxValue) => {
    if (minValue === maxValue) {
        return "hsl(20 85% 55%)";
    }

    const ratio = (value - minValue) / (maxValue - minValue);
    const hue = 220 - (220 * ratio);
    const lightness = 72 - (20 * ratio);

    return `hsl(${hue} 85% ${lightness}%)`;
};

export const defaultLayerDefinitions = {
        streetOpenStreetMap: {
            url: "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            options: {
                maxZoom: 18,
                subdomains: "abc",
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }
        },
        street: {
            url: "https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png",
            options: {
                maxZoom: 19,
                subdomains: "abc",
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Tiles style by <a href="https://www.hotosm.org/">Humanitarian OpenStreetMap Team</a>'
            }
        },
        streetAlternate: {
            url: "https://tile.openstreetmap.de/{z}/{x}/{y}.png",
            options: {
                maxZoom: 18,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
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

export const defaultLayerOrder = [
        "streetAlternate",
        "street",
        "terrain",
        "publicTransport",
        "streetOpenStreetMap"
    ];

export const createWarningElement = (message) => {
    const tileWarning = document.createElement("div");
    tileWarning.textContent = message;
    tileWarning.style.position = "absolute";
    tileWarning.style.left = "12px";
    tileWarning.style.right = "12px";
    tileWarning.style.bottom = "12px";
    tileWarning.style.zIndex = "1000";
    tileWarning.style.padding = "10px 12px";
    tileWarning.style.borderRadius = "8px";
    tileWarning.style.background = "rgba(31, 41, 55, 0.9)";
    tileWarning.style.color = "#fff";
    tileWarning.style.fontSize = "12px";
    tileWarning.style.display = "none";
    document.body.appendChild(tileWarning);
    return tileWarning;
};

export const fitBounds = (map, bounds, singlePointZoom = 6, padding = [30, 30]) => {
    if (!bounds || bounds.length === 0) {
        return;
    }

    if (bounds.length === 1) {
        map.setView(bounds[0], singlePointZoom);
        return;
    }

    map.fitBounds(bounds, { padding });
};

export const bindZoomScaledCircleMarkers = (map, markers, getRadius) => {
    const updateMarkerSizes = () => {
        const radius = getRadius(map.getZoom());

        for (const marker of markers) {
            marker.setRadius(radius);
        }
    };

    map.on("zoomend", updateMarkerSizes);
    updateMarkerSizes();

    return updateMarkerSizes;
};

export const getShapeStyle = (shape) => {
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

export const createColoredIcon = (markerColor, markerShape, markerInitials = "", markerOpacity = 0.85, markerSize = 18) => {
    
    const color = markerColor ?? "#198754";
    const fillColor = toRGBA(color, markerOpacity ?? 0.85);
    const glowColor = toRGBA(color, 0.55);
    const shape = markerShape ?? "Circle";
    const initials = String(markerInitials ?? "").slice(0, 3);
    const shapeStyle = getShapeStyle(shape);
    const iconSize = markerSize ?? 18; // getMarkerIconSizeFromZoom(zoom);
    const iconAnchor = Math.round(iconSize / 2);
    const scale = iconSize / 18;
    const label = initials
        ? `<span style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:8px;font-weight:700;color:#fff;transform:${shape === "Diamond" ? "rotate(-45deg)" : "none"};">${escapeHtml(initials)}</span>`
        : "";

    const baseHtml = shape === "Triangle"
        ? `<div style="position:relative;width:18px;height:18px;"><div style="${shapeStyle}${fillColor};filter:drop-shadow(0 0 2px ${glowColor});"></div><span style="position:absolute;left:50%;top:65%;transform:translate(-50%,-50%);font-size:7px;font-weight:700;color:#fff;">${escapeHtml(initials)}</span></div>`
        : `<div style="position:relative;width:18px;height:18px;"><div style="position:absolute;left:2px;top:2px;width:14px;height:14px;${shapeStyle}background:${fillColor};border:2px solid #fff;box-shadow:0 0 0 2px ${glowColor};">${label}</div></div>`;

    const html = `<div style="position:relative;width:${iconSize}px;height:${iconSize}px;"><div style="position:absolute;left:50%;top:50%;width:18px;height:18px;transform:translate(-50%,-50%) scale(${scale});transform-origin:center center;">${baseHtml}</div></div>`;

    return L.divIcon({
        className: "tracker-map-marker",
        html,
        iconSize: [iconSize, iconSize],
        iconAnchor: [iconAnchor, iconAnchor],
        markerColor: color,
        markerShape: shape,
        markerInitials: initials
    });
};

export const bindMarkerTooltip = (marker, item) => {
    const tooltipContent = item.name ? String(item.name) : "";
    const tooltip = marker.getTooltip?.();

    if (!tooltipContent) {
        if (tooltip) {
            marker.unbindTooltip();
        }

        return;
    }

    if (tooltip) {
        tooltip.setContent(tooltipContent);
        return;
    }

    marker.bindTooltip(tooltipContent, {
        direction: "top",
        // offset: [0, -14],
        opacity: 0.95
    });
};

export const bindMarkerPopup = (marker, item) => {
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

const updateTooltipToggleButton = (toggleButton, visible) => {
    if (!toggleButton) {
        return;
    }

    toggleButton.textContent = visible ? "Hide Labels" : "Show Labels";
    toggleButton.title = visible ? "Hide map labels" : "Show map labels";
};

const createTooltipToggleControl = (getTooltipsVisible, toggleTooltips, setToggleButton) => L.Control.extend({
    options: {
        position: "topright"
    },
    onAdd: () => {
        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control");
        const toggleButton = L.DomUtil.create("a", "", container);

        setToggleButton(toggleButton);
        toggleButton.href = "#";
        toggleButton.setAttribute("role", "button");
        toggleButton.setAttribute("aria-label", "Toggle map labels");
        toggleButton.style.width = "auto";
        toggleButton.style.padding = "0 10px";
        toggleButton.style.cursor = "pointer";

        updateTooltipToggleButton(toggleButton, getTooltipsVisible());

        L.DomEvent.disableClickPropagation(container);
        L.DomEvent.on(toggleButton, "click", (e) => {
            L.DomEvent.stop(e);
            toggleTooltips();
        });

        return container;
    }
});


export const createMap = (options = {}) => {
    const layerDefinitions = options.layerDefinitions ?? defaultLayerDefinitions;
    const layerOrder = options.layerOrder ?? defaultLayerOrder;
    const tileWarning = createWarningElement(options.tileWarningMessage ?? "Map tiles are unavailable right now.");
    const map = L.map(options.mapElementId ?? "map", {
        zoomControl: options.zoomControl ?? true,
        attributionControl: options.attributionControl ?? true
    });

    let currentLayerIndex = 0;
    let currentTileLayer = null;
    let tileErrors = 0;
    let tooltipsVisible = options.tooltipsVisible ?? false;
    let tooltipToggleButton = null;
    const tooltipMarkers = new Set();

    const showTileWarning = () => {
        tileWarning.style.display = "block";
    };

    const hideTileWarning = () => {
        tileWarning.style.display = "none";
    };

    const applyTooltipVisibility = (marker = null) => {
        const markers = marker ? [marker] : tooltipMarkers;

        for (const tooltipMarker of markers) {
            if (!tooltipMarker?.getTooltip?.()) {
                continue;
            }

            if (tooltipsVisible) {
                tooltipMarker.openTooltip();
            } else {
                tooltipMarker.closeTooltip();
            }
        }
    };

    const setTooltipVisibility = (visible) => {
        tooltipsVisible = visible;
        applyTooltipVisibility();
        updateTooltipToggleButton(tooltipToggleButton, visible);
    };

    const registerTooltipMarker = (marker) => {
        if (!marker) {
            return marker;
        }

        if (marker.getTooltip?.()) {
            tooltipMarkers.add(marker);
            applyTooltipVisibility(marker);
        }

        return marker;
    };

    const unregisterTooltipMarker = (marker) => {
        if (!marker) {
            return;
        }

        tooltipMarkers.delete(marker);
        marker.closeTooltip?.();
    };

    const attachTileLayer = (layerId) => {
        const definition = layerDefinitions[layerId];

        if (!definition) {
            return;
        }

        if (currentTileLayer) {
            map.removeLayer(currentTileLayer);
        }

        hideTileWarning();
        tileErrors = 0;
        currentTileLayer = L.tileLayer(definition.url, definition.options);
        currentTileLayer.on("tileload", () => {
            tileErrors = 0;
            hideTileWarning();
        });

        currentTileLayer.on("tileerror", () => {
            tileErrors += 1;

            if (tileErrors < 3) {
                return;
            }

            const nextLayerId = layerOrder[currentLayerIndex + 1];

            if (nextLayerId) {
                currentLayerIndex += 1;
                attachTileLayer(nextLayerId);
                return;
            }

            showTileWarning();
        });

        currentTileLayer.addTo(map);
    };

    attachTileLayer(layerOrder[currentLayerIndex]);
    map.addControl(new (createTooltipToggleControl(
        () => tooltipsVisible,
        () => setTooltipVisibility(!tooltipsVisible),
        (toggleButton) => {
            tooltipToggleButton = toggleButton;
            updateTooltipToggleButton(tooltipToggleButton, tooltipsVisible);
        }
    ))());

    return {
        map,
        fitBounds: (bounds, singlePointZoom, padding) => fitBounds(map, bounds, singlePointZoom, padding),
        bindZoomScaledCircleMarkers: (markers, getRadius) => bindZoomScaledCircleMarkers(map, markers, getRadius),
        registerTooltipMarker,
        unregisterTooltipMarker,
        applyTooltipVisibility,
        setTooltipVisibility,
        getTooltipsVisible: () => tooltipsVisible,
        showTileWarning,
        hideTileWarning,
        attachTileLayer
    };
};
