/**
 * Scroll-based URL hash updates
 * Updates URL hash as user scrolls - CSS :target handles active styling
 */
export function initActiveNavigation() {
  const navLinks =
    document.querySelectorAll<HTMLAnchorElement>(".sidebar-link");
  const sections = document.querySelectorAll<HTMLElement>("section[id]");

  if (navLinks.length === 0 || sections.length === 0) return;

  // Map section IDs to their corresponding nav link IDs
  // This handles nested sections (e.g., #dog-parks should map to #voorzieningen)
  const sectionIdToNavId = new Map<string, string>();
  sections.forEach((section) => {
    const sectionId = section.id;
    const link = document.querySelector<HTMLAnchorElement>(
      `.sidebar-link[href="#${sectionId}"]`
    );
    if (link) {
      // Direct mapping: section has a nav link
      sectionIdToNavId.set(sectionId, sectionId);
    } else {
      // Nested section: find parent section that has a nav link
      let parent = section.parentElement;
      while (parent) {
        if (parent.tagName === "SECTION" && parent.id) {
          const parentLink = document.querySelector<HTMLAnchorElement>(
            `.sidebar-link[href="#${parent.id}"]`
          );
          if (parentLink) {
            sectionIdToNavId.set(sectionId, parent.id);
            break;
          }
        }
        parent = parent.parentElement;
      }
    }
  });

  // Determine which section should be active based on scroll position
  function getActiveSectionId(): string | null {
    // Account for sticky headers (match CSS scroll-margin-top values)
    const headerOffset = window.innerWidth >= 768 ? 84 : 120;
    const viewportTop = window.scrollY + headerOffset;

    let activeSection: HTMLElement | null = null;
    let closestDistance = Infinity;

    // Find the section whose top is closest to (but above) the viewport top
    for (let i = 0; i < sections.length; i++) {
      const section = sections[i];
      const rect = section.getBoundingClientRect();
      const sectionTop = window.scrollY + rect.top;

      // Only consider sections that are at or above the viewport top
      // and within a reasonable distance (within 200px below viewport top)
      const distance = Math.abs(sectionTop - viewportTop);

      if (sectionTop <= viewportTop + 200 && distance < closestDistance) {
        closestDistance = distance;
        activeSection = section;
      }
    }

    // If no section found above viewport, use the first visible section
    if (!activeSection) {
      for (let i = 0; i < sections.length; i++) {
        const section = sections[i];
        const rect = section.getBoundingClientRect();
        if (rect.top < window.innerHeight && rect.bottom > 0) {
          activeSection = section;
          break;
        }
      }
    }

    // Map section ID to nav link ID (handles nested sections)
    if (activeSection) {
      const navId = sectionIdToNavId.get(activeSection.id);
      return navId || null;
    }

    // Default to first nav link's target if no section found
    if (navLinks.length > 0) {
      const firstLinkHash = navLinks[0].href.split("#")[1];
      return firstLinkHash || null;
    }

    return null;
  }

  // Update URL hash based on scroll position (without triggering scroll)
  let ticking = false;
  let lastHash = window.location.hash;

  function updateHashFromScroll() {
    if (!ticking) {
      window.requestAnimationFrame(() => {
        const activeSectionId = getActiveSectionId();
        // Always set a hash (never empty) - default to first section if needed
        const newHash = activeSectionId
          ? `#${activeSectionId}`
          : navLinks.length > 0
          ? navLinks[0].hash
          : "";

        // Only update hash if it changed
        if (newHash !== lastHash) {
          // Use replaceState to update URL without triggering scroll or adding history entry
          history.replaceState(null, "", newHash || window.location.pathname);
          lastHash = newHash;
        }
        ticking = false;
      });
      ticking = true;
    }
  }

  // Listen to scroll events to update hash
  window.addEventListener("scroll", updateHashFromScroll, { passive: true });

  // Update hash on initial load based on scroll position
  updateHashFromScroll();
}
