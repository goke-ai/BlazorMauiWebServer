// import { clearMarkers, initializeMap, renderAsync, setMarkersJson } from "./weather-map.js";

export async function renderMap(MapScriptFileName, __MAP_CONFIG_NAME__, __MAP_BRIDGE_NAME__, markers, emptyMessage, tileWarningMessage) {

    window[__MAP_CONFIG_NAME__] = {
        markers: markers,
        emptyMessage: emptyMessage,
        tileWarningMessage: tileWarningMessage
    };

    const mapScript = MapScriptFileName;

    let { clearMarkers, initializeMap, renderAsync, setMarkersJson } = await import(mapScript);


    window[__MAP_BRIDGE_NAME__] = {
        isReady: false,
        initialize(options) {
            return initializeMap(options ?? window[__MAP_CONFIG_NAME__] ?? {});
        },
        setMarkersJson(mapDataJson, options) {
            return setMarkersJson(mapDataJson, options ?? window[__MAP_CONFIG_NAME__] ?? {});
        },
        clear(options) {
            return clearMarkers(options ?? window[__MAP_CONFIG_NAME__] ?? {});
        }
    };

    renderAsync()
        .then(() => {
            window[__MAP_BRIDGE_NAME__].isReady = true;
        })
        .catch(() => {
            window[__MAP_BRIDGE_NAME__].isReady = false;
            document.body.innerHTML = '<div class="empty">Map script is unavailable.</div>';
        });
}


export const setMarkers = (__MAP_CONFIG_NAME__, __MAP_BRIDGE_NAME__, markers, options = {}) => {
    const bridge = window[__MAP_BRIDGE_NAME__];
    if (!bridge || bridge.isReady !== true) return 'not-ready';
    bridge.setMarkersJson(markers, { ...(window[__MAP_CONFIG_NAME__] ?? {}), fitBounds: true }); return 'ready';
};

export const clear = (__MAP_CONFIG_NAME__, __MAP_BRIDGE_NAME__) => {
    const bridge = window[__MAP_BRIDGE_NAME__];
    if (!bridge || bridge.isReady !== true) return 'not-ready';
    bridge.clear(); return 'ready';
};

//export function updateMarkers(markers, options = {}) {
//    const nextOptions = {
//        ...(window.weatherMapConfig ?? {}),
//        ...options,
//        markers
//    };

//    window.weatherMapConfig = nextOptions;

//    return setMarkersJson(markers, nextOptions);
//}

//export function clearMap(options = {}) {
//    const nextOptions = {
//        ...(window.weatherMapConfig ?? {}),
//        ...options,
//        markers: []
//    };

//    window.weatherMapConfig = nextOptions;

//    return clearMarkers(nextOptions);
//}
