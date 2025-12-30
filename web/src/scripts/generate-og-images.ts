/**
 * OG Image Generation Script
 * Generates dynamic Open Graph images for neighborhood pages using Satori + Resvg
 *
 * Run with: npx tsx src/scripts/generate-og-images.ts
 */

import satori from 'satori';
import { Resvg } from '@resvg/resvg-js';
import { readFileSync, writeFileSync, mkdirSync, existsSync, readdirSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';
import { config } from 'dotenv';

// Load environment variables
config();

// Get the directory of the current script
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const ROOT_DIR = join(__dirname, '..', '..');

// Site URL from environment (used for display on OG images)
const SITE_URL = process.env.PUBLIC_SITE_URL || 'https://www.buurtkompas.be';
const SITE_DISPLAY_NAME = SITE_URL.replace(/^https?:\/\//, ''); // Remove protocol for display

// Load neighborhood data from Content Collections (JSON files)
interface NeighborhoodData {
  id: string;
  name: string;
  city: string;
}

function loadNeighborhoods(): NeighborhoodData[] {
  const neighborhoodsDir = join(ROOT_DIR, 'src', 'content', 'neighborhoods');
  const files = readdirSync(neighborhoodsDir).filter(f => f.endsWith('.json'));

  return files.map(file => {
    const content = readFileSync(join(neighborhoodsDir, file), 'utf-8');
    return JSON.parse(content) as NeighborhoodData;
  });
}

const neighborhoods = loadNeighborhoods();

// Brand colors from _variables.scss
const COLORS = {
  background: '#faf4e8',
  text: '#2a2a2a',
  textGrey: '#4a4a4a',
  secondary: '#6a8f4e',
  primary: '#ff7a70',
};

// Load Poppins font
async function loadFonts() {
  const poppinsRegular = await fetch(
    'https://fonts.gstatic.com/s/poppins/v21/pxiEyp8kv8JHgFVrFJA.ttf'
  ).then((res) => res.arrayBuffer());

  const poppinsSemiBold = await fetch(
    'https://fonts.gstatic.com/s/poppins/v21/pxiByp8kv8JHgFVrLEj6V1s.ttf'
  ).then((res) => res.arrayBuffer());

  const poppinsBold = await fetch(
    'https://fonts.gstatic.com/s/poppins/v21/pxiByp8kv8JHgFVrLCz7V1s.ttf'
  ).then((res) => res.arrayBuffer());

  return [
    {
      name: 'Poppins',
      data: poppinsRegular,
      weight: 400 as const,
      style: 'normal' as const,
    },
    {
      name: 'Poppins',
      data: poppinsSemiBold,
      weight: 600 as const,
      style: 'normal' as const,
    },
    {
      name: 'Poppins',
      data: poppinsBold,
      weight: 700 as const,
      style: 'normal' as const,
    },
  ];
}

// Load paw prints image as base64
function loadPawPrintsImage(): string {
  const imagePath = join(ROOT_DIR, 'public', 'images', 'stamp_texture_paw_prints 1.png');
  const imageBuffer = readFileSync(imagePath);
  return `data:image/png;base64,${imageBuffer.toString('base64')}`;
}

// Generate OG image for a neighborhood
async function generateNeighborhoodOG(
  name: string,
  city: string,
  slug: string,
  fonts: Awaited<ReturnType<typeof loadFonts>>,
  pawPrintsBase64: string
): Promise<void> {
  const svg = await satori(
    {
      type: 'div',
      props: {
        style: {
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: COLORS.background,
          position: 'relative',
        },
        children: [
          // Site name at top
          {
            type: 'span',
            props: {
              style: {
                position: 'absolute',
                top: 50,
                fontSize: 28,
                color: COLORS.primary,
              },
              children: SITE_DISPLAY_NAME,
            },
          },
          // Main content - neighborhood name
          {
            type: 'div',
            props: {
              style: {
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
              },
              children: [
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 72,
                      fontWeight: 700,
                      color: COLORS.text,
                      textAlign: 'center',
                      maxWidth: 1000,
                    },
                    children: name,
                  },
                },
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 36,
                      color: COLORS.textGrey,
                      marginTop: 16,
                    },
                    children: city,
                  },
                },
              ],
            },
          },
          // Paw prints decoration (bottom right)
          {
            type: 'img',
            props: {
              src: pawPrintsBase64,
              width: 220,
              height: 110,
              style: {
                position: 'absolute',
                bottom: 40,
                right: 50,
              },
            },
          },
        ],
      },
    },
    {
      width: 1200,
      height: 630,
      fonts,
    }
  );

  // Convert SVG to PNG
  const resvg = new Resvg(svg, {
    fitTo: {
      mode: 'width',
      value: 1200,
    },
  });
  const pngData = resvg.render();
  const pngBuffer = pngData.asPng();

  // Write to file
  const outputPath = join(ROOT_DIR, 'public', 'og', `${slug}.png`);
  writeFileSync(outputPath, pngBuffer);
  console.log(`  ✓ Generated: ${slug}.png`);
}

// Generate default OG image (for home, city pages)
async function generateDefaultOG(
  fonts: Awaited<ReturnType<typeof loadFonts>>,
  pawPrintsBase64: string
): Promise<void> {
  const svg = await satori(
    {
      type: 'div',
      props: {
        style: {
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: COLORS.background,
          position: 'relative',
        },
        children: [
          // Main content
          {
            type: 'div',
            props: {
              style: {
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
              },
              children: [
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 64,
                      fontWeight: 700,
                      color: COLORS.primary,
                    },
                    children: SITE_DISPLAY_NAME,
                  },
                },
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 32,
                      color: COLORS.textGrey,
                      marginTop: 20,
                      textAlign: 'center',
                      maxWidth: 800,
                    },
                    children: 'Ontdek hondvriendelijke buurten in Gent',
                  },
                },
              ],
            },
          },
          // Paw prints decoration
          {
            type: 'img',
            props: {
              src: pawPrintsBase64,
              width: 220,
              height: 110,
              style: {
                position: 'absolute',
                bottom: 40,
                right: 50,
              },
            },
          },
        ],
      },
    },
    {
      width: 1200,
      height: 630,
      fonts,
    }
  );

  const resvg = new Resvg(svg, {
    fitTo: {
      mode: 'width',
      value: 1200,
    },
  });
  const pngData = resvg.render();
  const pngBuffer = pngData.asPng();

  const outputPath = join(ROOT_DIR, 'public', 'og', 'default.png');
  writeFileSync(outputPath, pngBuffer);
  console.log(`  ✓ Generated: default.png`);
}

// Generate city-specific OG image
async function generateCityOG(
  city: string,
  fonts: Awaited<ReturnType<typeof loadFonts>>,
  pawPrintsBase64: string
): Promise<void> {
  const svg = await satori(
    {
      type: 'div',
      props: {
        style: {
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: COLORS.background,
          position: 'relative',
        },
        children: [
          // Site name at top
          {
            type: 'span',
            props: {
              style: {
                position: 'absolute',
                top: 50,
                fontSize: 28,
                color: COLORS.primary,
              },
              children: SITE_DISPLAY_NAME,
            },
          },
          // Main content
          {
            type: 'div',
            props: {
              style: {
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
              },
              children: [
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 64,
                      fontWeight: 700,
                      color: COLORS.text,
                    },
                    children: `Buurten in ${city}`,
                  },
                },
                {
                  type: 'span',
                  props: {
                    style: {
                      fontSize: 32,
                      color: COLORS.textGrey,
                      marginTop: 20,
                    },
                    children: 'Ontdek hondvriendelijke wijken',
                  },
                },
              ],
            },
          },
          // Paw prints decoration
          {
            type: 'img',
            props: {
              src: pawPrintsBase64,
              width: 220,
              height: 110,
              style: {
                position: 'absolute',
                bottom: 40,
                right: 50,
              },
            },
          },
        ],
      },
    },
    {
      width: 1200,
      height: 630,
      fonts,
    }
  );

  const resvg = new Resvg(svg, {
    fitTo: {
      mode: 'width',
      value: 1200,
    },
  });
  const pngData = resvg.render();
  const pngBuffer = pngData.asPng();

  const outputPath = join(ROOT_DIR, 'public', 'og', `${city.toLowerCase()}.png`);
  writeFileSync(outputPath, pngBuffer);
  console.log(`  ✓ Generated: ${city.toLowerCase()}.png`);
}

// Main function
async function main() {
  console.log('\n🖼️  Generating OG images...\n');

  // Ensure output directory exists
  const outputDir = join(ROOT_DIR, 'public', 'og');
  if (!existsSync(outputDir)) {
    mkdirSync(outputDir, { recursive: true });
  }

  // Load resources
  console.log('  Loading fonts...');
  const fonts = await loadFonts();

  console.log('  Loading paw prints image...');
  const pawPrintsBase64 = loadPawPrintsImage();

  // Generate default image
  console.log('\n  Generating default image...');
  await generateDefaultOG(fonts, pawPrintsBase64);

  // Generate city image
  console.log('\n  Generating city images...');
  await generateCityOG('Gent', fonts, pawPrintsBase64);

  // Generate neighborhood images
  console.log('\n  Generating neighborhood images...');
  const neighborhoodList = Object.values(neighborhoods);

  for (const neighborhood of neighborhoodList) {
    await generateNeighborhoodOG(
      neighborhood.name,
      neighborhood.city,
      neighborhood.id,
      fonts,
      pawPrintsBase64
    );
  }

  console.log(`\n✅ Generated ${neighborhoodList.length + 2} OG images\n`);
}

main().catch((error) => {
  console.error('Error generating OG images:', error);
  process.exit(1);
});
