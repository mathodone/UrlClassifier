def to_ngrams(n, terms):
    if len(terms) <= n:
        return terms
    if n == 1:
        n_grams = [[term] for term in terms]
    else:
        n_grams = []
        for i in range(0, len(terms)-n+1):
            n_grams.append(terms[i:i+n])
    return n_grams

print(to_ngrams(3,"i like apple juice its good happy face".split(' ')))
print(to_ngrams(1,"i like apple juice its good happy face".split(' ')))

def add_doc(self, doc_id = '', doc_terms = [], doc_length=-1):
    if doc_length == -1:
        self.update_lengths(doc_id = doc_id, doc_length=len(doc_terms))
    else:
        self.update_lengths(doc_id = doc_id, doc_length=int(doc_length))

    for term in doc_terms:
        self.vocabulary.add(term)

    terms = self.lr_padding(doc_terms)
    ngrams = self.to_ngrams(terms)
    self.update_counts(doc_id, ngrams)

def term2ch(text):
    return [ch for ch in text]

#print(to_ngrams(2, term2ch("backstreets back ALRIGHT")))
print(term2ch("backsreet is bacc alright"))
print(term2ch("backstreet is bacc alright".split(' ')))