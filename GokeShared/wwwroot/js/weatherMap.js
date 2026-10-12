export function renderWeatherMap(element, forecasts, scaleMinTemperature, scaleMaxTemperature) {
    if (!element || !window.L) {
        return;
    }

    if (element._leafletMap) {
        element._leafletMap.remove();
        element._leafletMap = null;
    }

    if (!forecasts || forecasts.length === 0) {
        return;
    }

    //const minTemp = Math.min(...forecasts.map(f => f.temperatureC));
    //const maxTemp = Math.max(...forecasts.map(f => f.temperatureC));

    const minTemp = scaleMinTemperature ?? Math.min(...forecasts.map(f => f.temperatureC));
    const maxTemp = scaleMaxTemperature ?? Math.max(...forecasts.map(f => f.temperatureC));

    const getColor = (temp) => {
        if (minTemp === maxTemp) {
            return "hsl(20 85% 55%)";
        }

        const ratio = (temp - minTemp) / (maxTemp - minTemp);
        const hue = 220 - (220 * ratio);
        const lightness = 72 - (20 * ratio);

        return `hsl(${hue} 85% ${lightness}%)`;
    };

    const map = L.map(element);
    element._leafletMap = map;

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 18,
        attribution: "&copy; OpenStreetMap contributors"
    }).addTo(map);

    const bounds = [];
    const markers = [];
    let tooltipsVisible = false;
    let toggleButton;

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
            toggleButton.textContent = "Hide Temps";
            toggleButton.title = "Hide temperature labels";
            toggleButton.setAttribute("role", "button");
            toggleButton.setAttribute("aria-label", "Toggle temperature labels");
            toggleButton.style.width = "auto";
            toggleButton.style.padding = "0 10px";
            toggleButton.style.cursor = "pointer";

            L.DomEvent.disableClickPropagation(container);
            L.DomEvent.on(toggleButton, "click", (e) => {
                L.DomEvent.stop(e);
                setTooltipVisibility(!tooltipsVisible);
            });

            return container;
        }
    });

    map.addControl(new TooltipToggleControl());

    for (const forecast of forecasts) {
        const latLng = [forecast.latitude, forecast.longitude];
        bounds.push(latLng);

        const marker = L.circleMarker(latLng, {
            radius: 16,
            color: "#1f2937",
            weight: 2,
            fillColor: getColor(forecast.temperatureC),
            fillOpacity: 0.85
        }).addTo(map);

        marker.bindTooltip(`${forecast.temperatureC}&deg;C`, {
            permanent: true,
            direction: "top",
            offset: [0, -14],
            opacity: 0.95
        });

        marker.bindPopup(
            `<strong>${forecast.city}</strong><br/>
             Temperature: ${forecast.temperatureC}&deg;C<br/>
             Summary: ${forecast.summary ?? ""}<br/>
             Time: ${forecast.date}`
        );

        markers.push(marker);
    }

    if (bounds.length === 1) {
        map.setView(bounds[0], 6);
    } else {
        map.fitBounds(bounds, { padding: [30, 30] });
    }

    setTooltipVisibility(false);
}