import folium

# Create simple test map
m = folium.Map(location=[50.85, 4.35], zoom_start=10)

# Add a test marker
folium.Marker(
    location=[50.85, 4.35],
    popup="Test point",
    tooltip="Click me"
).add_to(m)

# Add test JavaScript
test_js = '''
<script>
console.log("=== JavaScript Test Starting ===");

setTimeout(function() {
    console.log("Map object:", typeof map);
    console.log("Window.map:", typeof window.map);

    // Try to find the map in different ways
    var foundMap = null;

    // Method 1: Check if map is a global
    if (typeof map !== 'undefined') {
        foundMap = map;
        console.log("Found map as global variable");
    }

    // Method 2: Check window.map
    if (!foundMap && typeof window.map !== 'undefined') {
        foundMap = window.map;
        console.log("Found map as window.map");
    }

    // Method 3: Look for Leaflet map instances
    if (!foundMap) {
        var mapDivs = document.querySelectorAll('.folium-map');
        console.log("Found map divs:", mapDivs.length);
        if (mapDivs.length > 0) {
            var mapId = mapDivs[0].id;
            console.log("Map div ID:", mapId);

            // Try to get map from Leaflet
            if (typeof window[mapId] !== 'undefined') {
                foundMap = window[mapId];
                console.log("Found map by ID:", mapId);
            }
        }
    }

    if (foundMap) {
        console.log("✓ Successfully found map object!");
        console.log("Map center:", foundMap.getCenter());

        // Test: Draw a line
        var testLine = L.polyline(
            [[50.85, 4.35], [50.86, 4.36]],
            {color: 'red', weight: 3}
        ).addTo(foundMap);

        console.log("✓ Test line drawn");

        // Test: Add click handler
        foundMap.on('click', function(e) {
            console.log("Map clicked at:", e.latlng);
            alert("Clicked at: " + e.latlng.lat + ", " + e.latlng.lng);
        });

        console.log("✓ Click handler added");
    } else {
        console.log("✗ Could not find map object");
    }

    console.log("=== JavaScript Test Complete ===");
}, 2000);
</script>
'''

# Save map
m.save('test_map.html')

# Inject JavaScript
with open('test_map.html', 'r', encoding='utf-8') as f:
    html = f.read()

html = html.replace('</body>', test_js + '</body>')

with open('test_map.html', 'w', encoding='utf-8') as f:
    f.write(html)

print("Test map created: test_map.html")
print("\nInstructions:")
print("1. Open test_map.html in your browser")
print("2. Open Developer Console (F12)")
print("3. Look at console output")
print("4. You should see a red test line")
print("5. Click anywhere - should see alert")
print("\nPlease share what you see in the console!")
