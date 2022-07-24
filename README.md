# jackhenry
Twitter API stream reporting

Capture incoming stream of 1% tweets using Twitter API and log the following stats to the console:
  >Total time elapsed;
  >Total number of tweets processed;
  >Top N number of top hashtags
  
appsettings.json file in the root solution folder contains the necessary customizations:
  >Twitter API base url;
  >Interval in seconds to publish stats to console;
  >Number of top hashtags to display;
  >Twitter API bearer token for authentication
