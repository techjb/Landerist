# baseline_tfidf.py
import pandas as pd
from pathlib import Path
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline
from sklearn.metrics import classification_report, roc_auc_score, average_precision_score
from sklearn.utils.class_weight import compute_class_weight
import joblib

DATA_DIR = Path("data")

def load_split(name):
    df = pd.read_csv(DATA_DIR / f"{name}.csv")
    # normaliza label a 0/1
    df["label"] = df["label"].astype(int)
    return df

def main():
    train = load_split("train")
    valid = load_split("valid")
    test  = load_split("test")

    # Calcula pesos por si hay desbalance
    classes = [0,1]
    class_weight = compute_class_weight("balanced", classes=classes, y=train["label"])
    cw = {c:w for c,w in zip(classes, class_weight)}

    pipe = Pipeline([
        ("tfidf", TfidfVectorizer(
            sublinear_tf=True,
            strip_accents="unicode",
            lowercase=True,
            ngram_range=(1,2),   # uni+bi-gramas
            max_df=0.9,
            min_df=5,
            max_features=200_000
        )),
        ("clf", LogisticRegression(
            solver="liblinear",
            class_weight=cw,
            max_iter=2000,
            n_jobs=1
        ))
    ])

    pipe.fit(train["text"], train["label"])

    for split_name, df in [("valid", valid), ("test", test)]:
        probs = pipe.predict_proba(df["text"])[:,1]
        preds = (probs >= 0.5).astype(int)
        print(f"\n== {split_name.upper()} ==")
        print(classification_report(df["label"], preds, digits=3))
        try:
            print("ROC-AUC:", round(roc_auc_score(df["label"], probs), 4))
            print("PR-AUC :", round(average_precision_score(df["label"], probs), 4))
        except:
            pass

    # Guarda el modelo y un umbral óptimo en valid (si prefieres F1)
    from sklearn.metrics import f1_score
    best_thr, best_f1 = 0.5, -1.0
    for thr in [i/100 for i in range(10, 91)]:
        f1 = f1_score(valid["label"], (pipe.predict_proba(valid["text"])[:,1] >= thr).astype(int))
        if f1 > best_f1:
            best_f1, best_thr = f1, thr
    print(f"\nUmbral óptimo (valid) para F1: {best_thr} (F1={best_f1:.4f})")

    joblib.dump({"model": pipe, "threshold": best_thr}, "tfidf_logreg.joblib")
    print("\nGuardado: tfidf_logreg.joblib")

if __name__ == "__main__":
    main()
