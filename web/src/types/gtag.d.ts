interface Window {
  gtag?: (
    command: 'event' | 'config' | 'js',
    targetId: string | Date,
    config?: Record<string, any>
  ) => void;
  dataLayer?: any[];
}
