// @ts-check
import { defineConfig } from 'astro/config';
import sitemap from '@astrojs/sitemap';

// https://astro.build/config
export default defineConfig({
  // TODO: Update to production URL (e.g., https://indebuurt.be)
  site: 'https://indebuurt.be',
  output: 'static',
  integrations: [sitemap()],
  build: {
    // Clean output directory before build
    format: 'directory',
  },
});