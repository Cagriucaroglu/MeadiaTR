---
description: Apply OptimatePlatform coding rules and retrieve syntax standards from memory
allowed-tools: [mcp__memory__search_nodes, mcp__csMcp__validate_syntax, mcp__csMcp__find_symbol, Read, Bash]
argument-hint: <coding-task>
---

# OptimatePlatform Kod Yazım Kuralları

CRITICAL: Memory'den syntax rules getir ve uygula!

## 🚫 KRİTİK YASAKLAR
- ❌ **var keyword** - Always explicit types
- ❌ **underscore prefixes** - Use primary constructors  
- ❌ **DateTime.Now/UtcNow** - Use DateTimeProvider
- ❌ **#pragma warning disable** - AI yasak!
- ❌ **String.Empty** - Use ""
- ❌ **Newtonsoft.Json** - System.Text.Json only

## ✅ ZORUNLU PATTERN'LER
- ✅ **Collection expressions**: [], [..list]
- ✅ **ConfigureAwait(false)** in libraries
- ✅ **ArgumentNullException.ThrowIfNull**
- ✅ **PascalCase acronyms**: Neo4J not Neo4j

## 🔍 SÜREÇ
1. Memory search: Benzer kod var mı?
2. CsMcp ile duplication kontrol
3. Syntax validation yap
4. Rules'a uygun yaz
5. Türkçe `<summary>` ekle

$ARGUMENTS