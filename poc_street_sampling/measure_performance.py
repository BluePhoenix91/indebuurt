"""
Story 10: Measure and Document Performance
Measures processing times and file sizes for street sampling POC
"""

import os
import time
import pandas as pd
import subprocess
from datetime import datetime

def get_file_size_mb(filepath):
    """Get file size in MB"""
    if os.path.exists(filepath):
        return os.path.getsize(filepath) / (1024 * 1024)
    return 0

def run_with_timing(script_name, phase_name):
    """Run a Python script and measure its execution time"""
    print(f"\n{'='*70}")
    print(f"Running: {phase_name}")
    print(f"{'='*70}")

    start_time = time.time()

    try:
        result = subprocess.run(
            ['python', script_name],
            capture_output=True,
            text=True,
            timeout=600  # 10 minute timeout
        )

        end_time = time.time()
        duration = end_time - start_time

        # Print output
        if result.stdout:
            print(result.stdout)

        # Check for errors
        if result.returncode != 0:
            print(f"ERROR: Script failed with return code {result.returncode}")
            if result.stderr:
                print(f"STDERR: {result.stderr}")
            return duration, False

        print(f"\n{phase_name} completed in {duration:.2f} seconds")
        return duration, True

    except subprocess.TimeoutExpired:
        print(f"ERROR: {phase_name} timed out after 10 minutes")
        return 600, False
    except Exception as e:
        print(f"ERROR: {phase_name} failed with exception: {e}")
        return 0, False

def count_sample_points_per_neighborhood():
    """Count sample points per neighborhood"""
    samples_df = pd.read_csv('data/samples/street_samples.csv')
    counts = samples_df.groupby('neighborhood_name').size().sort_values(ascending=False)
    return counts

def main():
    print("="*70)
    print("Street Sampling POC - Performance Measurement")
    print("="*70)
    print(f"Started at: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")

    # Track all timings
    timings = {}

    # Phase 1: Extract streets
    print("\n\nPHASE 1: STREET EXTRACTION")
    duration, success = run_with_timing('extract_streets.py', 'Street Extraction')
    timings['street_extraction'] = duration

    if not success:
        print("WARNING: Street extraction failed, continuing with existing data...")

    # Phase 2: Generate sample points
    print("\n\nPHASE 2: SAMPLE POINT GENERATION")
    duration, success = run_with_timing('generate_sample_points.py', 'Sample Point Generation')
    timings['sample_generation'] = duration

    if not success:
        print("WARNING: Sample generation failed, continuing with existing data...")

    # Phase 3: Extract POIs
    print("\n\nPHASE 3: POI EXTRACTION")
    duration, success = run_with_timing('extract_pois.py', 'POI Extraction')
    timings['poi_extraction'] = duration

    if not success:
        print("WARNING: POI extraction failed, continuing with existing data...")

    # Phase 4: Calculate distances
    print("\n\nPHASE 4: DISTANCE CALCULATION")
    duration, success = run_with_timing('calculate_distances.py', 'Distance Calculation')
    timings['distance_calculation'] = duration

    if not success:
        print("WARNING: Distance calculation failed, continuing with existing data...")

    # Phase 5: Aggregate labels
    print("\n\nPHASE 5: LABEL AGGREGATION")
    duration, success = run_with_timing('aggregate_labels.py', 'Label Aggregation')
    timings['label_aggregation'] = duration

    if not success:
        print("WARNING: Label aggregation failed, continuing with existing data...")

    # Phase 6: Create map
    print("\n\nPHASE 6: MAP GENERATION")
    duration, success = run_with_timing('create_map_with_lines.py', 'Map Generation')
    timings['map_generation'] = duration

    if not success:
        print("WARNING: Map generation failed, continuing with existing data...")

    # Calculate total time
    total_time = sum(timings.values())

    # Gather file sizes
    print("\n\n" + "="*70)
    print("GATHERING FILE SIZE AND COUNT METRICS")
    print("="*70)

    file_sizes = {
        'neighborhoods': get_file_size_mb('data/neighborhoods.csv'),
        'streets': get_file_size_mb('data/streets/residential_streets.geojson'),
        'street_samples': get_file_size_mb('data/samples/street_samples.csv'),
        'supermarkets': get_file_size_mb('data/pois/supermarkets.csv'),
        'pt_stops': get_file_size_mb('data/pois/pt_stops.csv'),
        'green_spaces': get_file_size_mb('data/pois/green_spaces.csv'),
        'distances': get_file_size_mb('results/distances_per_sample.csv'),
        'labels_summary': get_file_size_mb('results/neighborhood_labels_summary.csv'),
        'html_map': get_file_size_mb('street_sampling_map.html')
    }

    # Count records
    neighborhoods_df = pd.read_csv('data/neighborhoods.csv')
    streets_df = pd.read_csv('data/streets/residential_streets_summary.csv')
    samples_df = pd.read_csv('data/samples/street_samples.csv')
    pois_supermarkets = pd.read_csv('data/pois/supermarkets.csv')
    pois_pt = pd.read_csv('data/pois/pt_stops.csv')
    pois_green = pd.read_csv('data/pois/green_spaces.csv')
    distances_df = pd.read_csv('results/distances_per_sample.csv')
    labels_df = pd.read_csv('results/neighborhood_labels_summary.csv')

    record_counts = {
        'neighborhoods': len(neighborhoods_df),
        'streets': len(streets_df),
        'street_samples': len(samples_df),
        'supermarkets': len(pois_supermarkets),
        'pt_stops': len(pois_pt),
        'green_spaces': len(pois_green),
        'total_pois': len(pois_supermarkets) + len(pois_pt) + len(pois_green),
        'distances': len(distances_df),
        'labels': len(labels_df)
    }

    # Sample points per neighborhood
    sample_counts = count_sample_points_per_neighborhood()

    # Generate performance report
    print("\n\n" + "="*70)
    print("GENERATING PERFORMANCE REPORT")
    print("="*70)

    report_lines = []
    report_lines.append("="*70)
    report_lines.append("STREET SAMPLING POC - PERFORMANCE REPORT")
    report_lines.append("="*70)
    report_lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    report_lines.append("")

    # Processing times
    report_lines.append("="*70)
    report_lines.append("1. PROCESSING TIMES")
    report_lines.append("="*70)
    report_lines.append("")
    report_lines.append(f"Phase 1 - Street Extraction:      {timings['street_extraction']:>8.2f} seconds")
    report_lines.append(f"Phase 2 - Sample Generation:      {timings['sample_generation']:>8.2f} seconds")
    report_lines.append(f"Phase 3 - POI Extraction:         {timings['poi_extraction']:>8.2f} seconds")
    report_lines.append(f"Phase 4 - Distance Calculation:   {timings['distance_calculation']:>8.2f} seconds")
    report_lines.append(f"Phase 5 - Label Aggregation:      {timings['label_aggregation']:>8.2f} seconds")
    report_lines.append(f"Phase 6 - Map Generation:         {timings['map_generation']:>8.2f} seconds")
    report_lines.append(f"{'-'*70}")
    report_lines.append(f"TOTAL PROCESSING TIME:            {total_time:>8.2f} seconds ({total_time/60:.2f} minutes)")
    report_lines.append("")

    # Record counts
    report_lines.append("="*70)
    report_lines.append("2. DATA VOLUMES")
    report_lines.append("="*70)
    report_lines.append("")
    report_lines.append(f"Neighborhoods:                    {record_counts['neighborhoods']:>8,}")
    report_lines.append(f"Street segments:                  {record_counts['streets']:>8,}")
    report_lines.append(f"Street sample points:             {record_counts['street_samples']:>8,}")
    report_lines.append(f"POIs - Supermarkets:              {record_counts['supermarkets']:>8,}")
    report_lines.append(f"POIs - Public Transport:          {record_counts['pt_stops']:>8,}")
    report_lines.append(f"POIs - Green Spaces:              {record_counts['green_spaces']:>8,}")
    report_lines.append(f"POIs - Total:                     {record_counts['total_pois']:>8,}")
    report_lines.append(f"Distance records:                 {record_counts['distances']:>8,}")
    report_lines.append(f"Neighborhood labels:              {record_counts['labels']:>8,}")
    report_lines.append("")

    # Sample points per neighborhood
    report_lines.append("="*70)
    report_lines.append("3. SAMPLE POINTS PER NEIGHBORHOOD")
    report_lines.append("="*70)
    report_lines.append("")
    for neighborhood, count in sample_counts.items():
        report_lines.append(f"  {neighborhood:<40} {count:>6,} samples")
    report_lines.append(f"{'-'*70}")
    report_lines.append(f"  {'TOTAL':<40} {sample_counts.sum():>6,} samples")
    report_lines.append(f"  {'AVERAGE':<40} {sample_counts.mean():>6.1f} samples")
    report_lines.append(f"  {'MEDIAN':<40} {sample_counts.median():>6.1f} samples")
    report_lines.append("")

    # File sizes
    report_lines.append("="*70)
    report_lines.append("4. OUTPUT FILE SIZES")
    report_lines.append("="*70)
    report_lines.append("")
    total_size = 0
    for file_key, size_mb in file_sizes.items():
        report_lines.append(f"  {file_key:<35} {size_mb:>8.2f} MB")
        total_size += size_mb
    report_lines.append(f"{'-'*70}")
    report_lines.append(f"  {'TOTAL':<35} {total_size:>8.2f} MB")
    report_lines.append("")

    # Scalability projections
    report_lines.append("="*70)
    report_lines.append("5. SCALABILITY PROJECTIONS")
    report_lines.append("="*70)
    report_lines.append("")

    neighborhoods_tested = record_counts['neighborhoods']

    # Belgium has approximately 19,000 neighborhoods (rough estimate based on 581 municipalities)
    # Let's use a more conservative estimate of ~1000 neighborhoods for initial rollout
    projection_counts = [50, 100, 500, 1000]

    for target_neighborhoods in projection_counts:
        scale_factor = target_neighborhoods / neighborhoods_tested
        projected_time = total_time * scale_factor
        projected_samples = record_counts['street_samples'] * scale_factor
        projected_size = total_size * scale_factor

        report_lines.append(f"For {target_neighborhoods:>4} neighborhoods (scale factor: {scale_factor:.1f}x):")
        report_lines.append(f"  - Estimated processing time:  {projected_time/60:>8.1f} minutes ({projected_time/3600:.2f} hours)")
        report_lines.append(f"  - Estimated sample points:    {projected_samples:>8,.0f}")
        report_lines.append(f"  - Estimated storage size:     {projected_size:>8.1f} MB ({projected_size/1024:.2f} GB)")
        report_lines.append("")

    report_lines.append("NOTE: These are linear projections. Actual performance may vary due to:")
    report_lines.append("  - Neighborhood density variations (urban vs rural)")
    report_lines.append("  - OSM data density differences")
    report_lines.append("  - System resource constraints")
    report_lines.append("  - Potential parallelization optimizations")
    report_lines.append("")

    # Performance characteristics
    report_lines.append("="*70)
    report_lines.append("6. PERFORMANCE CHARACTERISTICS")
    report_lines.append("="*70)
    report_lines.append("")

    samples_per_second = record_counts['street_samples'] / total_time if total_time > 0 else 0
    distances_per_second = record_counts['distances'] / timings['distance_calculation'] if timings['distance_calculation'] > 0 else 0

    report_lines.append(f"Throughput:")
    report_lines.append(f"  - Sample points generated:        {samples_per_second:>8.1f} samples/second")
    report_lines.append(f"  - Distance calculations:          {distances_per_second:>8.1f} calculations/second")
    report_lines.append(f"  - Neighborhoods processed:        {neighborhoods_tested / (total_time/60):>8.2f} neighborhoods/minute")
    report_lines.append("")

    report_lines.append(f"Bottlenecks (slowest phases):")
    sorted_timings = sorted(timings.items(), key=lambda x: x[1], reverse=True)
    for i, (phase, duration) in enumerate(sorted_timings[:3], 1):
        percentage = (duration / total_time * 100) if total_time > 0 else 0
        report_lines.append(f"  {i}. {phase:<30} {duration:>8.2f}s ({percentage:>5.1f}%)")
    report_lines.append("")

    # Recommendations
    report_lines.append("="*70)
    report_lines.append("7. RECOMMENDATIONS")
    report_lines.append("="*70)
    report_lines.append("")
    report_lines.append("For production deployment:")
    report_lines.append("  - Consider batch processing neighborhoods in parallel (multi-core)")
    report_lines.append("  - Implement incremental updates for OSM data (avoid full reprocessing)")
    report_lines.append("  - Use spatial database (PostGIS) for efficient distance queries")
    report_lines.append("  - Pre-compute and cache SmartLabels for common queries")
    report_lines.append("  - Monitor memory usage for large-scale processing")
    report_lines.append("")
    report_lines.append("="*70)
    report_lines.append("END OF REPORT")
    report_lines.append("="*70)

    # Print report to console
    report_text = "\n".join(report_lines)
    print("\n" + report_text)

    # Save to file
    output_file = 'results/performance_report.txt'
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(report_text)

    print(f"\nPerformance report saved to: {output_file}")

    # Also create CSV for easy analysis
    csv_data = {
        'phase': list(timings.keys()) + ['TOTAL'],
        'duration_seconds': list(timings.values()) + [total_time]
    }
    csv_df = pd.DataFrame(csv_data)
    csv_file = 'results/performance_metrics.csv'
    csv_df.to_csv(csv_file, index=False)
    print(f"Performance metrics CSV saved to: {csv_file}")

    print(f"\n{'='*70}")
    print("PERFORMANCE MEASUREMENT COMPLETE")
    print(f"{'='*70}")
    print(f"Total time: {total_time/60:.2f} minutes")

if __name__ == "__main__":
    main()
