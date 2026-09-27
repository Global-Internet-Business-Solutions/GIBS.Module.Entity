/* Module Script */
var GIBS = GIBS || {};

GIBS.Entity = GIBS.Entity || {};

GIBS.Entity._googleMapsLoadPromise = null;
GIBS.Entity._googleMapsMaps = GIBS.Entity._googleMapsMaps || {};
GIBS.Entity._googleMapsMarkers = GIBS.Entity._googleMapsMarkers || {};

GIBS.Entity._loadGoogleMapsApi = function (apiKey) {
    if (window.google && window.google.maps) {
        return Promise.resolve();
    }

    if (GIBS.Entity._googleMapsLoadPromise) {
        return GIBS.Entity._googleMapsLoadPromise;
    }

    GIBS.Entity._googleMapsLoadPromise = new Promise(function (resolve, reject) {
        var callbackName = "gibsEntityGoogleMapsLoaded";
        window[callbackName] = function () {
            resolve();
            try {
                delete window[callbackName];
            } catch (e) {
                window[callbackName] = undefined;
            }
        };

        var script = document.createElement("script");
        script.src = "https://maps.googleapis.com/maps/api/js?key=" + encodeURIComponent(apiKey) + "&callback=" + callbackName;
        script.async = true;
        script.defer = true;
        script.onerror = function () {
            reject(new Error("Failed to load Google Maps JavaScript API"));
        };

        document.head.appendChild(script);
    });

    return GIBS.Entity._googleMapsLoadPromise;
};

GIBS.Entity.renderMapPreview = function (elementId, latitude, longitude, zoomLevel, mapType, apiKey) {
    if (!elementId || !apiKey) {
        return Promise.resolve();
    }

    return GIBS.Entity._loadGoogleMapsApi(apiKey).then(function () {
        var el = document.getElementById(elementId);
        if (!el) {
            return;
        }

        var center = { lat: Number(latitude), lng: Number(longitude) };
        if (Number.isNaN(center.lat) || Number.isNaN(center.lng)) {
            return;
        }

        var options = {
            center: center,
            zoom: Number(zoomLevel) || 10,
            mapTypeId: mapType || "roadmap",
            zoomControl: true,
            mapTypeControl: true,
            streetViewControl: true,
            fullscreenControl: true
        };

        var map = GIBS.Entity._googleMapsMaps[elementId];
        var marker = GIBS.Entity._googleMapsMarkers[elementId];
        var needsRecreate = !map || (typeof map.getDiv === "function" && map.getDiv() !== el);

        if (needsRecreate) {
            el.innerHTML = "";
            map = new google.maps.Map(el, options);
            GIBS.Entity._googleMapsMaps[elementId] = map;
            marker = new google.maps.Marker({ position: center, map: map });
            GIBS.Entity._googleMapsMarkers[elementId] = marker;
        } else {
            map.setOptions(options);
            map.setCenter(center);

            if (!marker) {
                marker = new google.maps.Marker({ position: center, map: map });
                GIBS.Entity._googleMapsMarkers[elementId] = marker;
            } else {
                marker.setMap(map);
                marker.setPosition(center);
            }
        }
    });
};
