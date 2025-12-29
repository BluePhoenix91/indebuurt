// @ts-check
import { defineConfig } from 'astro/config';
import sitemap from '@astrojs/sitemap';
import { loadEnv } from 'vite';

const { PUBLIC_SITE_URL } = loadEnv(process.env.NODE_ENV ?? 'production', process.cwd(), '');
const siteUrl = PUBLIC_SITE_URL || 'https://www.buurtkompas.be';

// https://astro.build/config
export default defineConfig({
  site: siteUrl,
  output: 'static',
  trailingSlash: 'always',
  integrations: [
    sitemap({
      // Add priority and change frequency hints for search engines
      changefreq: 'daily', // Active development phase
      priority: 0.8,
      lastmod: new Date(),
      serialize(item) {
        // Homepage gets highest priority
        if (item.url === `${siteUrl}/`) {
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