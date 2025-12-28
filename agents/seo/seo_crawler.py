"""
SEO Crawler for buurtkompas.be
Analyzes SEO factors across the website
"""

import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse
import json
from typing import Dict, List, Set
import time
from collections import defaultdict

class SEOCrawler:
    def __init__(self, base_url: str = "https://www.buurtkompas.be"):
        self.base_url = base_url
        self.visited_urls: Set[str] = set()
        self.page_data: List[Dict] = []
        self.internal_links: Set[str] = set()
        self.external_links: Set[str] = set()
        self.errors: List[Dict] = []
        
    def is_internal_link(self, url: str) -> bool:
        """Check if URL is internal to the site"""
        parsed = urlparse(url)
        base_parsed = urlparse(self.base_url)
        return parsed.netloc == base_parsed.netloc or parsed.netloc == ""
    
    def normalize_url(self, url: str) -> str:
        """Normalize URL (remove fragments, trailing slashes)"""
        parsed = urlparse(url)
        # Remove fragment
        normalized = f"{parsed.scheme}://{parsed.netloc}{parsed.path}"
        if parsed.query:
            normalized += f"?{parsed.query}"
        # Remove trailing slash except for root
        if normalized != self.base_url and normalized.endswith('/'):
            normalized = normalized[:-1]
        return normalized
    
    def extract_links(self, soup: BeautifulSoup, current_url: str) -> Set[str]:
        """Extract all links from a page"""
        links = set()
        for link in soup.find_all('a', href=True):
            href = link['href']
            absolute_url = urljoin(current_url, href)
            normalized = self.normalize_url(absolute_url)
            
            # Skip fragments, mailto, tel, javascript
            if any(normalized.startswith(prefix) for prefix in ['mailto:', 'tel:', 'javascript:', '#']):
                continue
                
            if self.is_internal_link(normalized):
                links.add(normalized)
            else:
                self.external_links.add(normalized)
        return links
    
    def analyze_page(self, url: str) -> Dict:
        """Analyze a single page for SEO factors"""
        try:
            response = requests.get(url, timeout=10, allow_redirects=True)
            response.raise_for_status()
            
            soup = BeautifulSoup(response.content, 'html.parser')
            
            # Extract meta tags
            title_tag = soup.find('title')
            title = title_tag.get_text(strip=True) if title_tag else None
            
            meta_description = soup.find('meta', attrs={'name': 'description'})
            description = meta_description.get('content', '') if meta_description else None
            
            # Open Graph tags
            og_title = soup.find('meta', property='og:title')
            og_title_content = og_title.get('content', '') if og_title else None
            
            og_description = soup.find('meta', property='og:description')
            og_description_content = og_description.get('content', '') if og_description else None
            
            og_image = soup.find('meta', property='og:image')
            og_image_content = og_image.get('content', '') if og_image else None
            
            # Canonical URL
            canonical = soup.find('link', rel='canonical')
            canonical_url = canonical.get('href', '') if canonical else None
            
            # H1 tags
            h1_tags = [h1.get_text(strip=True) for h1 in soup.find_all('h1')]
            
            # H2 tags
            h2_tags = [h2.get_text(strip=True) for h2 in soup.find_all('h2')]
            
            # Schema.org JSON-LD
            schema_scripts = soup.find_all('script', type='application/ld+json')
            schema_data = []
            for script in schema_scripts:
                try:
                    schema_data.append(json.loads(script.string))
                except:
                    pass
            
            # Images with alt text
            images = soup.find_all('img')
            images_with_alt = sum(1 for img in images if img.get('alt'))
            images_without_alt = len(images) - images_with_alt
            
            # Links
            links = self.extract_links(soup, url)
            self.internal_links.update(links)
            
            # Word count (approximate)
            text_content = soup.get_text()
            word_count = len(text_content.split())
            
            # Check for common SEO issues
            issues = []
            if not title:
                issues.append("Missing title tag")
            elif len(title) > 60:
                issues.append(f"Title too long ({len(title)} chars, recommended: 50-60)")
            elif len(title) < 30:
                issues.append(f"Title too short ({len(title)} chars, recommended: 50-60)")
            
            if not description:
                issues.append("Missing meta description")
            elif len(description) > 160:
                issues.append(f"Description too long ({len(description)} chars, recommended: 150-160)")
            elif len(description) < 120:
                issues.append(f"Description too short ({len(description)} chars, recommended: 150-160)")
            
            if len(h1_tags) == 0:
                issues.append("Missing H1 tag")
            elif len(h1_tags) > 1:
                issues.append(f"Multiple H1 tags ({len(h1_tags)})")
            
            if images_without_alt > 0:
                issues.append(f"{images_without_alt} images missing alt text")
            
            if not canonical_url:
                issues.append("Missing canonical URL")
            
            if word_count < 300:
                issues.append(f"Low word count ({word_count} words, recommended: 300+)")
            
            return {
                'url': url,
                'status_code': response.status_code,
                'title': title,
                'meta_description': description,
                'og_title': og_title_content,
                'og_description': og_description_content,
                'og_image': og_image_content,
                'canonical': canonical_url,
                'h1_count': len(h1_tags),
                'h1_tags': h1_tags,
                'h2_count': len(h2_tags),
                'h2_tags': h2_tags[:5],  # First 5 H2s
                'schema_count': len(schema_data),
                'schema_types': [s.get('@type', 'Unknown') for s in schema_data],
                'images_total': len(images),
                'images_with_alt': images_with_alt,
                'images_without_alt': images_without_alt,
                'internal_links': len(links),
                'word_count': word_count,
                'issues': issues,
                'response_size': len(response.content)
            }
            
        except Exception as e:
            self.errors.append({'url': url, 'error': str(e)})
            return None
    
    def crawl(self, start_url: str = None, max_pages: int = 50, use_sitemap: bool = True):
        """Crawl the website starting from a URL"""
        to_visit = []
        
        # Try to get URLs from sitemap first
        if use_sitemap:
            print("Fetching URLs from sitemap...")
            sitemap_urls = self.parse_sitemap()
            if sitemap_urls:
                print(f"Found {len(sitemap_urls)} URLs in sitemap")
                to_visit.extend(sitemap_urls[:max_pages])
            else:
                print("No sitemap found, starting from homepage")
                if start_url is None:
                    start_url = self.base_url
                to_visit.append(start_url)
        else:
            if start_url is None:
                start_url = self.base_url
            to_visit.append(start_url)
        
        while to_visit and len(self.page_data) < max_pages:
            url = to_visit.pop(0)
            normalized = self.normalize_url(url)
            
            if normalized in self.visited_urls:
                continue
                
            self.visited_urls.add(normalized)
            print(f"Analyzing ({len(self.page_data)+1}/{max_pages}): {normalized}")
            
            page_data = self.analyze_page(normalized)
            if page_data:
                self.page_data.append(page_data)
                
                # Add internal links to queue (only if we haven't reached max and not using sitemap)
                if not use_sitemap and len(self.page_data) < max_pages:
                    for link in self.internal_links:
                        link_normalized = self.normalize_url(link)
                        if link_normalized not in self.visited_urls and link_normalized.startswith(self.base_url):
                            to_visit.append(link_normalized)
            
            time.sleep(0.5)  # Be polite
    
    def check_robots_txt(self) -> Dict:
        """Check robots.txt"""
        try:
            response = requests.get(f"{self.base_url}/robots.txt", timeout=5)
            return {
                'exists': response.status_code == 200,
                'content': response.text if response.status_code == 200 else None,
                'status_code': response.status_code
            }
        except Exception as e:
            return {'exists': False, 'error': str(e)}
    
    def check_sitemap(self) -> Dict:
        """Check for sitemap"""
        sitemap_urls = [
            f"{self.base_url}/sitemap.xml",
            f"{self.base_url}/sitemap-index.xml",
            f"{self.base_url}/sitemap.txt"
        ]
        
        results = {}
        for url in sitemap_urls:
            try:
                response = requests.get(url, timeout=5)
                if response.status_code == 200:
                    results[url] = {
                        'exists': True,
                        'content_type': response.headers.get('Content-Type', ''),
                        'size': len(response.content)
                    }
            except:
                pass
        
        return results
    
    def parse_sitemap(self) -> List[str]:
        """Parse sitemap to get list of URLs"""
        urls = []
        
        # Try sitemap-index first
        try:
            response = requests.get(f"{self.base_url}/sitemap-index.xml", timeout=5)
            if response.status_code == 200:
                soup = BeautifulSoup(response.content, 'xml')
                # Find all sitemap references
                sitemap_refs = soup.find_all('sitemap')
                for ref in sitemap_refs:
                    loc = ref.find('loc')
                    if loc:
                        sitemap_url = loc.get_text(strip=True)
                        # Fetch the actual sitemap
                        try:
                            sitemap_response = requests.get(sitemap_url, timeout=5)
                            if sitemap_response.status_code == 200:
                                sitemap_soup = BeautifulSoup(sitemap_response.content, 'xml')
                                url_tags = sitemap_soup.find_all('url')
                                for url_tag in url_tags:
                                    loc_tag = url_tag.find('loc')
                                    if loc_tag:
                                        urls.append(loc_tag.get_text(strip=True))
                        except:
                            pass
        except:
            pass
        
        # If no sitemap-index, try regular sitemap
        if not urls:
            try:
                response = requests.get(f"{self.base_url}/sitemap.xml", timeout=5)
                if response.status_code == 200:
                    soup = BeautifulSoup(response.content, 'xml')
                    url_tags = soup.find_all('url')
                    for url_tag in url_tags:
                        loc_tag = url_tag.find('loc')
                        if loc_tag:
                            urls.append(loc_tag.get_text(strip=True))
            except:
                pass
        
        return urls
    
    def generate_report(self) -> Dict:
        """Generate comprehensive SEO report"""
        # Overall statistics
        total_pages = len(self.page_data)
        pages_with_issues = sum(1 for p in self.page_data if p.get('issues'))
        
        # Common issues
        issue_counts = defaultdict(int)
        for page in self.page_data:
            for issue in page.get('issues', []):
                issue_type = issue.split('(')[0].strip()
                issue_counts[issue_type] += 1
        
        # Average metrics
        avg_word_count = sum(p.get('word_count', 0) for p in self.page_data) / total_pages if total_pages > 0 else 0
        avg_internal_links = sum(p.get('internal_links', 0) for p in self.page_data) / total_pages if total_pages > 0 else 0
        
        # Pages missing key elements
        missing_title = sum(1 for p in self.page_data if not p.get('title'))
        missing_description = sum(1 for p in self.page_data if not p.get('meta_description'))
        missing_h1 = sum(1 for p in self.page_data if p.get('h1_count', 0) == 0)
        missing_canonical = sum(1 for p in self.page_data if not p.get('canonical'))
        
        # Schema.org usage
        pages_with_schema = sum(1 for p in self.page_data if p.get('schema_count', 0) > 0)
        
        return {
            'summary': {
                'total_pages_analyzed': total_pages,
                'total_internal_links_found': len(self.internal_links),
                'total_external_links_found': len(self.external_links),
                'pages_with_issues': pages_with_issues,
                'errors': len(self.errors)
            },
            'metrics': {
                'average_word_count': round(avg_word_count, 0),
                'average_internal_links_per_page': round(avg_internal_links, 1),
                'pages_with_schema_markup': pages_with_schema,
                'schema_usage_percentage': round((pages_with_schema / total_pages * 100) if total_pages > 0 else 0, 1)
            },
            'issues': {
                'missing_title': missing_title,
                'missing_meta_description': missing_description,
                'missing_h1': missing_h1,
                'missing_canonical': missing_canonical,
                'common_issues': dict(sorted(issue_counts.items(), key=lambda x: x[1], reverse=True)[:10])
            },
            'technical_seo': {
                'robots_txt': self.check_robots_txt(),
                'sitemap': self.check_sitemap()
            },
            'pages': self.page_data,
            'errors': self.errors
        }

if __name__ == "__main__":
    crawler = SEOCrawler()
    print("Starting SEO crawl of buurtkompas.be...")
    crawler.crawl(max_pages=50, use_sitemap=True)
    
    print("\nGenerating report...")
    report = crawler.generate_report()
    
    # Save report with timestamp
    import os
    from datetime import datetime
    timestamp = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')
    report_filename = f'seo_analysis_report_{timestamp}.json'
    report_path = os.path.join(os.path.dirname(__file__), report_filename)
    with open(report_path, 'w', encoding='utf-8') as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    
    # Print summary
    print("\n" + "="*60)
    print("SEO ANALYSIS SUMMARY")
    print("="*60)
    print(f"Pages analyzed: {report['summary']['total_pages_analyzed']}")
    print(f"Internal links found: {report['summary']['total_internal_links_found']}")
    print(f"Pages with issues: {report['summary']['pages_with_issues']}")
    print(f"\nAverage word count: {report['metrics']['average_word_count']}")
    print(f"Pages with schema markup: {report['metrics']['pages_with_schema_markup']} ({report['metrics']['schema_usage_percentage']}%)")
    print(f"\nMissing title tags: {report['issues']['missing_title']}")
    print(f"Missing meta descriptions: {report['issues']['missing_meta_description']}")
    print(f"Missing H1 tags: {report['issues']['missing_h1']}")
    print(f"Missing canonical URLs: {report['issues']['missing_canonical']}")
    print(f"\nReport saved to: {report_path}")

