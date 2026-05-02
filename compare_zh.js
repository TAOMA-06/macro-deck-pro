const fs = require("fs");
const en = JSON.parse(fs.readFileSync("C:/Users/34355/Desktop/Macro-Deck pro/MacroDeck/Resources/Languages/English.json", "utf8"));
const zh = JSON.parse(fs.readFileSync("C:/Users/34355/Desktop/Macro-Deck pro/MacroDeck/Resources/Languages/Chinese.json", "utf8"));

const enKeys = Object.keys(en);
const zhKeys = Object.keys(zh);

const missing = enKeys.filter(k => !(k in zh));
const extra = zhKeys.filter(k => !(k in en));
const identical = [];

for (const k of enKeys) {
    if (k in zh && en[k] === zh[k]) {
        identical.push(k);
    }
}

console.log("=== ENGLISH.json keys: " + enKeys.length);
console.log("=== CHINESE.json keys: " + zhKeys.length);
console.log("");

console.log("=== MISSING FROM Chinese.json (" + missing.length + " keys) ===");
if (missing.length === 0) console.log("  (none)");
else missing.forEach(k => console.log("  MISSING: " + k));
console.log("");

console.log("=== EXTRA IN Chinese.json (" + extra.length + " keys) ===");
if (extra.length === 0) console.log("  (none)");
else extra.forEach(k => console.log("  EXTRA: " + k));
console.log("");

console.log("=== UNTRANSLATED (same value as English) (" + identical.length + " keys) ===");
if (identical.length === 0) console.log("  (none)");
else identical.forEach(k => console.log("  UNTRANSLATED: " + k + " = " + en[k].substring(0, 100)));
