---
description: OptimatePlatform NuGet package management with centralized dependency control
allowed-tools: [mcp__nuget-search__get-nuget-solver, mcp__nuget-search__get-nuget-solver-latest-versions, mcp__nuget-search__get-latest-package-version, Read, Write, Bash]
argument-hint: <package-name-or-action>
---

# OptimatePlatform NuGet Management

## 📦 Centralized Package Management
**Directory.Packages.props** controls all package versions centrally.

### **Update Strategies:**

#### **🔍 Vulnerability Check:**
```bash
# Check for vulnerable packages and get fixes
mcp__nuget-search__get-nuget-solver
```

#### **⬆️ Latest Compatible Versions:**
```bash  
# Update to latest compatible versions
mcp__nuget-search__get-nuget-solver-latest-versions
```

#### **🔎 Specific Package Latest:**
```bash
# Check latest version of specific package
mcp__nuget-search__get-latest-package-version
```

## 🚫 **OptimatePlatform Rules:**
- ❌ **No Preview Versions** unless critical
- ✅ **Stable Releases Only** for production
- 🔒 **Central Version Control** via Directory.Packages.props
- 📊 **Compatibility Check** with .NET 9 + Aspire
- 🛡️ **Security Priority** - vulnerability fixes first

## 🎯 **Process:**
1. **Analyze Current**: Read Directory.Packages.props
2. **Check Vulnerabilities**: Run solver for security issues
3. **Plan Updates**: Identify compatible latest versions
4. **Test Compatibility**: Ensure .NET 9 + Aspire support
5. **Update Centrally**: Modify Directory.Packages.props
6. **Verify Build**: Run dotnet build + tests

## 📋 **Common Actions:**
- `vulnerabilities` - Check for security issues
- `latest` - Get latest compatible versions  
- `specific Microsoft.AspNetCore.App` - Check specific package
- `update-all` - Full dependency update

<package-name-or-action> 