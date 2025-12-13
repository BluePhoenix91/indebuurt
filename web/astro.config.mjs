// @ts-check
import { defineConfig } from 'astro/config';
import sitemap from '@astrojs/sitemap';

// https://astro.build/config
export default defineConfig({
  site: 'https://buurtkompas.be',
  output: 'static',
  integrations: [
    sitemap({
      // Add priority and change frequency hints for search engines
      changefreq: 'daily', // Active development phase
      priority: 0.8,
      lastmod: new Date(),
      serialize(item) {
        // Homepage gets highest priority
        if (item.url === 'https://buurtkompas.be/') {
          item.priority = 1.0;
        }
        return item;
      },
    }),
  ],
  build: {
    // Clean output directory before build
    format: 'directory',
  },
});