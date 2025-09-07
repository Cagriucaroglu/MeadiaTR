---
description: Document OptimatePlatform work with memory persistence and structured logging
allowed-tools: [mcp__memory__create_entities, mcp__memory__create_relations, mcp__memory__add_observations, Write, Read, Bash]
argument-hint: <documentation-scope>
---

# OptimatePlatform Documentation Strategy

## 🧠 Memory-First Documentation
Prioritize persistent knowledge over files:

### **Knowledge Entities to Create:**
- **Architecture Changes** - New patterns, migrations
- **Implementation Progress** - Completed features, lessons learned  
- **Code Patterns** - Reusable solutions, best practices
- **Integration Points** - Service connections, dependencies
- **Performance Insights** - Optimizations, benchmarks

### **Relationship Mapping:**
- Link new entities to existing architecture
- Connect implementation to business requirements
- Map code changes to design decisions

## 📁 Fallback File Documentation
When memory insufficient or formal docs needed:

### **CLAUDE.md Updates:**
- Critical project-wide changes
- New coding standards
- Architecture decisions

### **Implementation Logs:**
- Session-specific progress
- Debug insights
- Problem-solution pairs
- File: `_Docs/Claude-Implementation-Log/YYYY-MM-DD-session-summary.md`

### **Formal Documentation:**
- Architecture updates in `_Docs/1-Analiz/`
- Implementation guides in `_Docs/2-Implementation/`

## 🎯 Usage Examples
`/write-docs "New MongoDB optimization patterns"`
`/write-docs "Authentication flow implementation"`
`/write-docs "Performance benchmarking results"`

$ARGUMENTS