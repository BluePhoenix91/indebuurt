/**
 * Pipeline configuration constants.
 *
 * These settings live in code rather than the database because:
 * - You're always working from Claude Code and can edit directly
 * - Paths are tied to repo structure (code convention, not runtime config)
 * - Changes are version-controlled and reviewable
 */

export const PIPELINE_CONFIG = {
  /** Base path for intermediate pipeline outputs (e.g., researcher, writer JSON files) */
  outputBasePath: 'agents/pipeline-outputs',

  /** Path for final published Astro content */
  contentOutputPath: 'web/src/content/neighborhoods',

  /** Minimum final_score (0-100) required for auto-publishing */
  qualityThreshold: 70,

  /** Maximum retry attempts for failed jobs */
  maxRetries: 3,
} as const;

/**
 * Pipeline stage definitions.
 * Order matters - this is the sequence jobs progress through.
 */
export const PIPELINE_STAGES = [
  'researcher',
  'writer',
  'seo-reviewer',
  'brand-reviewer',
] as const;

export type PipelineStage = (typeof PIPELINE_STAGES)[number];

/**
 * Get the output file path for a given stage and neighborhood.
 *
 * Convention: {outputBasePath}/{nis_code}/{stage_number}-{stage_name}.json
 *
 * @example
 * getOutputPath('41002A0', 'researcher')    // 'agents/pipeline-outputs/41002A0/1-researcher.json'
 * getOutputPath('44021A1', 'seo-reviewer')  // 'agents/pipeline-outputs/44021A1/3-seo-reviewer.json'
 */
export function getOutputPath(nisCode: string, stage: PipelineStage): string {
  const stageIndex = PIPELINE_STAGES.indexOf(stage) + 1;
  return `${PIPELINE_CONFIG.outputBasePath}/${nisCode}/${stageIndex}-${stage}.json`;
}

/**
 * Get the final content output path for a neighborhood.
 * Uses the slug (from brand-reviewer output `id` field) as the filename.
 *
 * @example
 * getContentPath('aalst-aalst-station') // 'web/src/content/neighborhoods/aalst-aalst-station.json'
 */
export function getContentPath(slug: string): string {
  return `${PIPELINE_CONFIG.contentOutputPath}/${slug}.json`;
}
