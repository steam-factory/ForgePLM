# ForgePLM Control Rules v1

## 1. Purpose

ForgePLM establishes a controlled system for managing engineering design data and production outputs within SolidWorks environments lacking full PLM/PDM enterprise capabilities.

The system enforces:

- **Traceability**
- **Revision discipline**
- **Separation of concerns**
- **Data integrity**

## 2. Core Doctrine

### 2.1 Engineering vs Production Separation

> **Engineering data creates production data. Production data never becomes engineering data.**

- Engineering data is **editable and controlled**
- Production data is **read-only and released**
- The two domains must **never overlap or substitute for each other**

### 2.2 Traceability Requirement

> **Every production artifact must map to exactly one discrete engineering source state.**

Each production record must include:

- Part Number
- Revision
- ECO
- Source File GUID
- Timestamp

## 3. Object Definitions

### 3.1 Engineering Record

Editable, controlled design data.

Includes:

- SolidWorks files (`.sldprt`, `.sldasm`, `.slddrw`)
- Custom properties (ForgePLM metadata)
- ECO linkage
- Revision state

### 3.2 Production Record

Released, read-only derivative outputs.

Includes:

- STEP files
- PDF drawings
- Approved STL/DXF (optional)

Characteristics:

- Immutable
- Non-editable
- Used for manufacturing only

### 3.3 Part Number

A persistent identity representing a design item.

Rules:

- Exists independent of file
- Can have multiple revisions
- Has exactly **one current revision**

### 3.4 Revision

A controlled version of a Part Number.

Rules:

- Created only under ECO
- Belongs to a **revision family (100 / 200 / 300)**
- Only one revision may be **active/current**

### 3.5 ECO (Engineering Change Order)

The **only mechanism** for:

- Creating new revisions
- Modifying existing items
- Promoting revision families (100 → 200 → 300)

### 3.6 Artifact

A generated output tied to a specific engineering state.

Examples:

- STL export
- STEP export
- PDF export

Artifacts must include:

- Incremental identifier
- Source revision
- Source file GUID

## 4. Revision System

### 4.1 Revision Families

ForgePLM uses structured revision families:

- **100-series** → Concept / Early Development
- **200-series** → Design Validation / Pre-Production
- **300-series** → Production Release

### 4.2 Revision Rules

- New part under ECO level starts at:
  - `100 → 101`
  - `200 → 201`
  - `300 → 301`

- Existing part revision increments within family:
  - `101 → 102 → 103`
  - `201 → 202`

### 4.3 Promotion Between Families

- Only via ECO
- Example:
  - `103 → 201` (promotion from 100 → 200 family)

### 4.4 Constraints

- No manual revision edits allowed
- No revision creation outside ECO
- Only one active revision per part

## 5. File Management Rules

### 5.1 CAD File Operations

All operations must be **SolidWorks-aware**.

Allowed:

- Save As
- Pack and Go
- API-based duplication/rename

Forbidden:

- OS-level file copy
- OS-level rename
- Manual file moves affecting references

### 5.2 Folder Structure

Each project contains:

```text
/ProjectName
    /sandbox
    /dev
    /prod
```

#### sandbox

- Uncontrolled / experimental

#### dev

- Active ECO work
- Editable CAD files
- PDM-controlled

#### prod

- Released outputs only
- Neutral formats (STEP, PDF)
- Read-only

## 6. Engineering vs Production Rules

### 6.1 Engineering Restrictions

- Must never use production files as source
- Must always originate from:
  - native CAD
  - controlled revision state

### 6.2 Production Restrictions

- Must never be edited
- Must never be overwritten
- Must only be generated from:
  - Released engineering revision

### 6.3 Release Rule

Production data can only be created when:

- Engineering revision is marked **Released**
- ECO is complete/approved

## 7. Safeguards (Hard Rules)

### 7.1 Revision Integrity

- A part cannot have more than one active revision

### 7.2 ECO Enforcement

- No revision creation outside ECO workflow

### 7.3 Property Control

- Revision values cannot be manually overridden in file properties

### 7.4 Artifact Traceability

- All artifacts must map to:
  - Part Number
  - Revision
  - ECO
  - Source File

### 7.5 File Operation Safety

- All CAD duplication/rename must use SolidWorks APIs

### 7.6 Production Immutability

- Production files are read-only after release

### 7.7 Domain Separation

- Engineering and Production data must never be interchanged

## 8. System Architecture Roles

### Properties Tab

- Live file context
- Read/write PLM properties
- Sync with database

### Project Tab

- Create/select project
- Validate folder structure
- Define working context

### ECO Tab (Primary Workflow Engine)

- Create ECO
- Assign level (100/200/300)
- Add part numbers
- Create/revise items
- Launch asset creation

### Part Numbers Tab

- Registry of parts
- Search and view
- Show revision history

## 9. Asset Creation Modes

All new assets must originate via controlled methods:

### 9.1 From Template

- Uses `base.sldprt`, `base.sldasm`, `base.slddrw`

### 9.2 Rename Active Document

- Converts working file into controlled asset

### 9.3 Copy Active Document

- Creates derived design with preserved source

## 10. Design Philosophy

ForgePLM is designed to be:

- **Strict where it matters**
- **Simple where possible**
- **Integrated with SolidWorks**
- **Traceable end-to-end**

## 11. Non-Negotiables

- No uncontrolled revisions
- No ambiguous file origins
- No mixing engineering and production data
- No breaking SolidWorks references via filesystem hacks
- No production without traceability

## 12. Closing Principle

> **If it cannot be traced, it does not exist.  
> If it can be edited, it is not production.  
> If it is not under ECO, it is not controlled.**
