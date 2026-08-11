window.rideoutMap = (() => {
    const maps = new Map();

    async function initialize(elementId, routeUrl, waypointUrl) {
        dispose(elementId);

        const element = document.getElementById(elementId);
        if (!element) throw new Error(`Kartenelement '${elementId}' wurde nicht gefunden.`);
        if (!window.L) throw new Error("Leaflet wurde nicht geladen.");

        const map = L.map(elementId, { zoomControl: true, scrollWheelZoom: false });
        maps.set(elementId, map);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "&copy; OpenStreetMap-Mitwirkende"
        }).addTo(map);

        const [routeResponse, waypointResponse] = await Promise.all([
            fetch(routeUrl, { cache: "no-store" }),
            fetch(waypointUrl, { cache: "no-store" })
        ]);

        if (!routeResponse.ok) throw new Error(`Routendatei konnte nicht geladen werden (${routeResponse.status}).`);
        if (!waypointResponse.ok) throw new Error(`Wegpunktdatei konnte nicht geladen werden (${waypointResponse.status}).`);

        const route = await routeResponse.json();
        const waypoints = await waypointResponse.json();
        const boundsLayers = [];
        const hasRoute = Array.isArray(route.features) && route.features.length > 0;

        if (hasRoute) {
            // Die dunkle, schmale Kontur trennt den Verlauf von Straßen und Flächen,
            // ohne die Basiskarte oder Ortsnamen großflächig zu verdecken.
            const routeOutline = L.geoJSON(route, {
                style: {
                    color: "#20105d",
                    weight: 8,
                    opacity: 0.88,
                    lineCap: "round",
                    lineJoin: "round"
                }
            }).addTo(map);

            const routeLine = L.geoJSON(route, {
                style: {
                    color: "#3b16d9",
                    weight: 5,
                    opacity: 1,
                    lineCap: "round",
                    lineJoin: "round"
                }
            }).addTo(map);

            boundsLayers.push(routeOutline, routeLine);
        }

        const visibleFeatures = mergeStartAndFinish(waypoints.features ?? []);
        const markerLayer = L.geoJSON({ type: "FeatureCollection", features: visibleFeatures }, {
            pointToLayer: (feature, latlng) => {
                const sequence = feature.properties?.sequence ?? "";
                const role = feature.properties?.role ?? "waypoint";
                const label = role === "start-finish" ? "S/Z" : sequence;
                const markerClass = role === "start-finish"
                    ? "rideout-waypoint rideout-waypoint--start-finish"
                    : "rideout-waypoint";

                const icon = L.divIcon({
                    className: "rideout-waypoint-wrapper",
                    html: `<span class="${markerClass}">${label}</span>`,
                    iconSize: [28, 28],
                    iconAnchor: [14, 14]
                });

                return L.marker(latlng, { icon, keyboard: true });
            },
            onEachFeature: (feature, layer) => {
                const name = feature.properties?.name ?? "Wegpunkt";
                const address = feature.properties?.address;
                const role = feature.properties?.role;
                const roleText = role === "start-finish" ? "Start und Ziel" : `Wegpunkt ${feature.properties?.sequence ?? ""}`;
                layer.bindPopup(`<strong>${escapeHtml(roleText)}</strong><br>${escapeHtml(name)}${address ? `<br>${escapeHtml(address)}` : ""}`);
            }
        }).addTo(map);

        boundsLayers.push(markerLayer);

        const group = L.featureGroup(boundsLayers);
        if (group.getBounds().isValid()) map.fitBounds(group.getBounds(), { padding: [34, 34] });
        else map.setView([52.9593381, 10.5464655], 12);

        setTimeout(() => map.invalidateSize(), 0);
        return hasRoute;
    }

    function mergeStartAndFinish(features) {
        const start = features.find(feature => feature.properties?.role === "start");
        const finish = features.find(feature => feature.properties?.role === "finish");
        const remaining = features.filter(feature => !["start", "finish"].includes(feature.properties?.role));

        if (!start) return features;

        const combined = structuredClone(start);
        combined.properties = {
            ...combined.properties,
            role: "start-finish",
            name: start.properties?.name ?? finish?.properties?.name ?? "Albrecht-Thaer-Gelände"
        };

        return [combined, ...remaining];
    }

    function dispose(elementId) {
        const map = maps.get(elementId);
        if (map) {
            map.remove();
            maps.delete(elementId);
        }
    }

    function escapeHtml(value) {
        return String(value).replace(/[&<>'"]/g, character => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "'": "&#39;",
            '"': "&quot;"
        })[character]);
    }

    return { initialize, dispose };
})();
