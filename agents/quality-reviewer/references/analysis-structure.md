# Analysis Object Structure

The `analysis` object in QualityReviewerOutput captures detailed debugging information for each scoring category.

## Structure

```json
{
  "analysis": {
    "terminology": {
      "avoidedTermsFound": ["buurt", "huisdier"],
      "preferredTermsPresent": ["baasjes", "hondenspeelweide", "wijk", "viervoeter"],
      "allowedExceptionsUsed": ["buurtgevoel", "in de buurt"]
    },
    "tone": {
      "perspectiveForm": "je_jouw",
      "formalPhrasesFound": [],
      "promotionalPhrasesFound": [],
      "friendlyMarkersCount": 12
    },
    "localAuthenticity": {
      "uniquePlaceNamesCount": 5,
      "localTipsFound": ["via de Dendermondsesteenweg", "richting de waterkant"],
      "neighborhoodObservations": ["levendige sfeer rond de Dendermondsesteenweg"]
    },
    "narrativeNaturalness": {
      "sentenceStartVariety": 0.82,
      "averageSentenceLength": 14.3,
      "listLikePatternsFound": 0
    },
    "sparseDataHandling": {
      "gapsDetected": ["no pet store in neighborhood"],
      "gapsHandledGracefully": 1,
      "gapsHandledPoorly": 0
    }
  }
}
```

## Field Descriptions

### terminology
| Field | Type | Description |
|-------|------|-------------|
| `avoidedTermsFound` | string[] | Terms from `avoid` arrays that were found and replaced |
| `preferredTermsPresent` | string[] | Preferred terms already correctly used |
| `allowedExceptionsUsed` | string[] | Phrases from `allowedPhrases` that matched |

### tone
| Field | Type | Description |
|-------|------|-------------|
| `perspectiveForm` | string | Primary perspective: `je_jouw`, `u_uw`, `wij_ons`, or `mixed` |
| `formalPhrasesFound` | string[] | Corporate/formal phrases detected |
| `promotionalPhrasesFound` | string[] | Marketing/promotional phrases detected |
| `friendlyMarkersCount` | number | Count of friendly tone indicators |

### localAuthenticity
| Field | Type | Description |
|-------|------|-------------|
| `uniquePlaceNamesCount` | number | Distinct place names in narrative text |
| `localTipsFound` | string[] | Phrases with local navigation tips |
| `neighborhoodObservations` | string[] | Specific character observations |

### narrativeNaturalness
| Field | Type | Description |
|-------|------|-------------|
| `sentenceStartVariety` | number | Ratio: unique first words / total sentences (0-1) |
| `averageSentenceLength` | number | Mean words per sentence |
| `listLikePatternsFound` | number | Count of repetitive patterns detected |

### sparseDataHandling
| Field | Type | Description |
|-------|------|-------------|
| `gapsDetected` | string[] | Missing amenity types identified |
| `gapsHandledGracefully` | number | Gaps with proper acknowledge-pivot-alternative |
| `gapsHandledPoorly` | number | Gaps with apologetic or dead-end handling |
