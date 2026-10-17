export function getCurrentLocation() {
    return new Promise(function (resolve, reject) {
        if (!navigator.geolocation) {
            reject(new Error("Geolocation is not supported by this browser."));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            function (position) {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    altitude: position.coords.altitude,
                    accuracy: position.coords.accuracy,
                    verticalAccuracy: position.coords.verticalAccuracy,
                    reducedAccuracy: position.coords.reducedAccuracy,
                    speed: position.coords.speed,
                    heading: position.coords.heading,
                    timestamp: position.timestamp
                });
            },
            function (error) {
                reject(new Error(error.message || "Unable to retrieve the current location."));
            },
            {
                enableHighAccuracy: true,
                maximumAge: 0,
                timeout: 15000
            });
    });
}

export async function getCurrentLocationAsync() {
    try {
        const location = await getCurrentLocation();
        return location;
    } catch (error) {
        throw error;
    }       
}

export function watchCurrentLocation(onUpdate, onError) {
    if (!navigator.geolocation) {
        if (onError) {
            onError(new Error("Geolocation is not supported by this browser."));
        }
        return null;
    }

    const watchId = navigator.geolocation.watchPosition(    
        function (position) {
            if (onUpdate) {
                onUpdate({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy,
                    speed: position.coords.speed,
                    heading: position.coords.heading,
                    timestamp: position.timestamp
                });
            }
        },
        function (error) {
            if (onError) {
                onError(new Error(error.message || "Unable to retrieve the current location."));
            }
        },
        {
            enableHighAccuracy: true,
            maximumAge: 0,
            timeout: 15000
        });
    return watchId;
}


