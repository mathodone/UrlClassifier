# UrlClassifier
C# Url Classifier

# TODO

* Clean up the code.
* Implement laplace mixing.

## What

Url Classifier I built while I was contracting with a b2b leads scraper/seller. The classifier was trained using a given training set of the form (URL, HAS_VALUE), i.e. if one wanted to train a classifier to determine if a given url was likely to contain addresses on the page, the training set would be something like ("www.example.com/locations", 1) and so forth.


## How

The classification algorithm is an adaptation of "URL-Based Web Page Classification: With n-Gram Language Models" by Abdallah and Iglesia (https://www.semanticscholar.org/paper/URL-based-Web-Page-Classification-A-New-Method-for-Abdallah-Iglesia/558ab095cd66e178f7b5d6e9f390b1675a03bed6).

## Results

In my particular use case, the classifier performed decently given the small size of the training sets it was fed (~300 urls each). Good recall; although some false positives occurred, it ended up being ok in practice because scraping a few extra pages isn't a big deal. I imagine training and testing the classifier with a much larger training set yields better results.
