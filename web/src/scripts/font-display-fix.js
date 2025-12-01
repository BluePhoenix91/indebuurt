// Post-build script to add font-display: swap to FontAwesome 7 fonts
// This fixes the font display issue reported by Lighthouse

import { readFileSync, writeFileSync, readdirSync, statSync } from 'fs';
import { join } from 'path';

function findFontAwesomeCSS(dir) {
  try {
    const files = readdirSync(dir);
    for (const file of files) {
      const filePath = join(dir, file);
      const stat = statSync(filePath);
      
      if (stat.isDirectory() && file === '_astro') {
        // Look in _astro directory
        const astroFiles = readdirSync(filePath);
        for (const astroFile of astroFiles) {
          if (astroFile.endsWith('.css') && astroFile.includes('all')) {
            return join(filePath, astroFile);
          }
        }
      } else if (stat.isFile() && file.endsWith('.css') && file.includes('all')) {
        return filePath;
      }
    }
  } catch (error) {
    return null;
  }
  return null;
}

const distPath = join(process.cwd(), 'dist');
const fontAwesomeCSSPath = findFontAwesomeCSS(distPath);

if (fontAwesomeCSSPath) {
  try {
    let css = readFileSync(fontAwesomeCSSPath, 'utf-8');
    
    // Add font-display: swap to all @font-face rules that don't have it
    css = css.replace(
      /@font-face\s*\{([^}]*)\}/g,
      (match, content) => {
        if (content.includes('font-display')) {
          // Already has font-display, replace it
          return match.replace(/font-display:\s*[^;]+;?/g, 'font-display: swap;');
        } else {
          // Add font-display before closing brace
          return `@font-face {${content}font-display: swap;}`;
        }
      }
    );
    
    writeFileSync(fontAwesomeCSSPath, css, 'utf-8');
    console.log('✓ Added font-display: swap to FontAwesome 7 fonts');
  } catch (error) {
    console.warn('Could not modify FontAwesome CSS:', error.message);
  }
} else {
  console.warn('Could not find FontAwesome CSS file in dist/');
}

