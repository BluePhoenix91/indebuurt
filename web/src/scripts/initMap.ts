import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix for default marker icon path issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
  iconUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",
  shadowUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
});

interface Marker {
  lat: number;
  lon: number;
  name: string;
  number?: number;
}

interface MapConfig {
  mapId: string;
  center: {
    lat: number;
    lon: number;
    zoom: number;
  };
  markers: Marker[];
}

export function initMap(config: MapConfig) {
  const mapContainer = document.querySelector(`[data-map-id="${config.mapId}"]`);
  if (!mapContainer) return;

  // Create map instance
  const map = L.map(mapContainer as HTMLElement).setView(
    [config.center.lat, config.center.lon],
    config.center.zoom
  );

  // Add OpenStreetMap tiles
  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution:
      '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    maxZoom: 19,
  }).addTo(map);

  // Add markers
  config.markers.forEach((marker, index) => {
    const markerNumber = marker.number ?? index + 1;

    // Create custom numbered marker
    const customIcon = L.divIcon({
      className: "custom-numbered-marker",
      html: `<div class="marker-number">${markerNumber}</div>`,
      iconSize: [30, 30],
      iconAnchor: [15, 30],
    });

    L.marker([marker.lat, marker.lon], { icon: customIcon })
      .addTo(map)
      .bindPopup(marker.name);
  });

  // Fit bounds to show all markers if there are any
  if (config.markers.length > 0) {
    const bounds = L.latLngBounds(
      config.markers.map((m) => [m.lat, m.lon] as [number, number])
    );
    map.fitBounds(bounds, { padding: [50, 50] });
  }
}



