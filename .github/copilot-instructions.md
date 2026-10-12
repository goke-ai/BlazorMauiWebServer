# Copilot Instructions

## Project Guidelines
- User prefers refactoring GokeApp auth cleanup by converting TokenStorage from static methods to an instance service.
- Simulated WeatherForecast data must remain stable for the same time period across requests.
- Original displayed temperatures must not change when applying weather map filtering controls; filters should only hide/show items, not alter the underlying temperature values or interpretation.
- User prefers weather.js to load map-core.js rather than injecting map-core.js separately from the HTML/template layer.
- User prefers using JavaScript export/import modules for map scripts instead of embedding one script into another or injecting shared script separately.