// @ts-check
import { defineConfig } from 'astro/config';
import sitemap from '@astrojs/sitemap';

// https://astro.build/config
export default defineConfig({
  site: 'https://buurtkompas.be',
  output: 'static',
  integrations: [sitemap()],
  build: {
    // Clean output directory before build
    format: 'directory',
  },
});