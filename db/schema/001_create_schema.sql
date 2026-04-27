/*
ForgePLM Database Schema

Creates ForgePLM database objects only: sequences, tables, views, constraints, indexes, and stored procedures.

This script intentionally does not create the database, database files, server logins, database users, or permissions.
Create/select the target database first, then run this script in that database context.

Do not add live operating data to this file. Static reference data belongs in db/seed.
*/

/****** Object:  Sequence [dbo].[seq_artifact_number]    ******/
CREATE SEQUENCE [dbo].[seq_artifact_number] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE 
GO
/****** Object:  Sequence [dbo].[seq_eco_number]    ******/
CREATE SEQUENCE [dbo].[seq_eco_number] 
 AS [int]
 START WITH 50
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE 
GO
/****** Object:  Sequence [dbo].[seq_part_number]    ******/
CREATE SEQUENCE [dbo].[seq_part_number] 
 AS [int]
 START WITH 500
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE 
GO
/****** Object:  Table [dbo].[eco]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eco](
	[eco_id] [int] IDENTITY(1,1) NOT NULL,
	[eco_number_int] [int] NOT NULL,
	[eco_number]  AS (right('00000'+CONVERT([varchar](10),[eco_number_int]),(5))) PERSISTED,
	[project_id] [int] NOT NULL,
	[eco_title] [nvarchar](255) NOT NULL,
	[eco_description] [nvarchar](max) NULL,
	[release_level] [int] NOT NULL,
	[eco_state] [nvarchar](30) NOT NULL,
	[closed_reason] [nvarchar](255) NULL,
	[closed_at] [datetime2](0) NULL,
	[created_at] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_eco] PRIMARY KEY CLUSTERED 
(
	[eco_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_eco_eco_number] UNIQUE NONCLUSTERED 
(
	[eco_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_eco_eco_number_int] UNIQUE NONCLUSTERED 
(
	[eco_number_int] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[part_numbers]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[part_numbers](
	[part_id] [int] IDENTITY(1,1) NOT NULL,
	[part_number_int] [int] NOT NULL,
	[part_number]  AS (right('0000000'+CONVERT([varchar](10),[part_number_int]),(7))) PERSISTED,
	[project_id] [int] NOT NULL,
	[description_current] [nvarchar](255) NULL,
	[current_revision_id] [int] NULL,
	[created_at] [datetime2](0) NOT NULL,
	[retired_at] [datetime2](0) NULL,
	[category_code] [char](2) NOT NULL,
	[document_type] [varchar](20) NOT NULL,
 CONSTRAINT [PK_part_numbers] PRIMARY KEY CLUSTERED 
(
	[part_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_part_numbers_part_number] UNIQUE NONCLUSTERED 
(
	[part_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_part_numbers_part_number_int] UNIQUE NONCLUSTERED 
(
	[part_number_int] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[revisions]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[revisions](
	[revision_id] [int] IDENTITY(1,1) NOT NULL,
	[part_id] [int] NOT NULL,
	[eco_id] [int] NOT NULL,
	[revision_code] [int] NOT NULL,
	[revision_family] [int] NOT NULL,
	[revision_seq] [int] NULL,
	[revision_description] [nvarchar](255) NULL,
	[part_description] [nvarchar](255) NULL,
	[material] [nvarchar](100) NULL,
	[unit] [nvarchar](50) NULL,
	[unit_cost] [decimal](18, 4) NULL,
	[unit_qty] [decimal](18, 4) NULL,
	[markup] [decimal](18, 4) NULL,
	[extended_price] [decimal](18, 4) NULL,
	[sale_price] [decimal](18, 4) NULL,
	[source] [nvarchar](255) NULL,
	[vendor] [nvarchar](255) NULL,
	[vendor_part_number] [nvarchar](100) NULL,
	[revise_flag] [bit] NOT NULL,
	[revised_at] [datetime2](0) NULL,
	[revision_state] [nvarchar](30) NOT NULL,
	[is_current] [bit] NOT NULL,
	[source_file_guid] [uniqueidentifier] NULL,
	[source_file_path] [nvarchar](500) NULL,
	[created_at] [datetime2](0) NOT NULL,
	[lifecycle_state] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_revisions] PRIMARY KEY CLUSTERED 
(
	[revision_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_revisions_part_revision_code] UNIQUE NONCLUSTERED 
(
	[part_id] ASC,
	[revision_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[artifacts]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[artifacts](
	[artifact_id] [int] IDENTITY(1,1) NOT NULL,
	[artifact_number_int] [int] NOT NULL,
	[artifact_number]  AS (right('000000'+CONVERT([varchar](10),[artifact_number_int]),(6))) PERSISTED,
	[revision_id] [int] NOT NULL,
	[eco_id] [int] NOT NULL,
	[artifact_type] [nvarchar](20) NOT NULL,
	[file_name] [nvarchar](255) NOT NULL,
	[file_path] [nvarchar](500) NOT NULL,
	[file_hash] [nvarchar](128) NULL,
	[source_file_guid] [uniqueidentifier] NULL,
	[generated_at] [datetime2](0) NOT NULL,
	[released_at] [datetime2](0) NULL,
	[is_read_only] [bit] NOT NULL,
 CONSTRAINT [PK_artifacts] PRIMARY KEY CLUSTERED 
(
	[artifact_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_artifacts_artifact_number] UNIQUE NONCLUSTERED 
(
	[artifact_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_artifacts_artifact_number_int] UNIQUE NONCLUSTERED 
(
	[artifact_number_int] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_artifact_traceability]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   VIEW [dbo].[v_artifact_traceability]
AS
SELECT
    a.artifact_id,
    a.artifact_number,
    a.artifact_type,
    a.file_name,
    a.file_path,
    a.file_hash,
    a.source_file_guid,
    a.generated_at,
    a.released_at,
    a.is_read_only,

    p.part_id,
    p.part_number,

    r.revision_id,
    r.revision_code,
    r.revision_family,
    r.lifecycle_state,
    r.revision_state,
    r.is_current,

    e.eco_id,
    e.eco_number,
    e.eco_state,
    e.release_level
FROM dbo.artifacts a
JOIN dbo.revisions r
    ON r.revision_id = a.revision_id
JOIN dbo.part_numbers p
    ON p.part_id = r.part_id
JOIN dbo.eco e
    ON e.eco_id = a.eco_id;
GO
/****** Object:  View [dbo].[v_current_revisions]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_current_revisions]
AS
SELECT
    p.part_id,
    p.part_number,
    p.part_number_int,
    p.project_id,
    p.category,
    r.revision_id,
    r.revision_code,
    r.revision_family,
    r.part_description,
    r.revision_description,
    r.revision_state,
    r.release_state,
    r.eco_id,
    e.eco_number,
    e.release_level,
    r.source_file_guid,
    r.source_file_path
FROM dbo.part_numbers p
JOIN dbo.revisions r
    ON p.current_revision_id = r.revision_id
JOIN dbo.eco e
    ON r.eco_id = e.eco_id;
GO
/****** Object:  View [dbo].[v_released_artifacts]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_released_artifacts]
AS
SELECT
    a.artifact_id,
    a.artifact_number,
    a.artifact_type,
    a.file_name,
    a.file_path,
    a.generated_at,
    a.released_at,
    r.revision_id,
    r.revision_code,
    p.part_id,
    p.part_number,
    e.eco_id,
    e.eco_number
FROM dbo.artifacts a
JOIN dbo.revisions r
    ON a.revision_id = r.revision_id
JOIN dbo.part_numbers p
    ON r.part_id = p.part_id
JOIN dbo.eco e
    ON a.eco_id = e.eco_id;
GO
/****** Object:  Table [dbo].[customers]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[customers](
	[customer_id] [int] IDENTITY(1,1) NOT NULL,
	[customer_code] [nvarchar](50) NOT NULL,
	[customer_name] [nvarchar](255) NOT NULL,
	[contact_name] [nvarchar](255) NULL,
	[contact_email] [nvarchar](255) NULL,
	[contact_phone] [nvarchar](50) NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[customer_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[customer_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[part_categories]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[part_categories](
	[category_code] [char](2) NOT NULL,
	[category_name] [nvarchar](255) NOT NULL,
	[guideline] [nvarchar](max) NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[category_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[projects]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[projects](
	[project_id] [int] IDENTITY(1,1) NOT NULL,
	[project_code] [nvarchar](50) NOT NULL,
	[project_name] [nvarchar](255) NOT NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](0) NOT NULL,
	[customer_id] [int] NOT NULL,
	[project_seq] [int] NOT NULL,
 CONSTRAINT [PK_projects] PRIMARY KEY CLUSTERED 
(
	[project_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_projects_customer_seq] UNIQUE NONCLUSTERED 
(
	[customer_id] ASC,
	[project_seq] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_projects_project_code] UNIQUE NONCLUSTERED 
(
	[project_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[users]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[users](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](100) NOT NULL,
	[display_name] [nvarchar](255) NOT NULL,
	[email] [nvarchar](255) NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_artifacts_eco_id]    ******/
CREATE NONCLUSTERED INDEX [IX_artifacts_eco_id] ON [dbo].[artifacts]
(
	[eco_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_artifacts_revision_id]    ******/
CREATE NONCLUSTERED INDEX [IX_artifacts_revision_id] ON [dbo].[artifacts]
(
	[revision_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_artifacts_revision_type_path]    ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_artifacts_revision_type_path] ON [dbo].[artifacts]
(
	[revision_id] ASC,
	[artifact_type] ASC,
	[file_path] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_eco_project_id]    ******/
CREATE NONCLUSTERED INDEX [IX_eco_project_id] ON [dbo].[eco]
(
	[project_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_part_numbers_project_id]    ******/
CREATE NONCLUSTERED INDEX [IX_part_numbers_project_id] ON [dbo].[part_numbers]
(
	[project_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_revisions_eco_id]    ******/
CREATE NONCLUSTERED INDEX [IX_revisions_eco_id] ON [dbo].[revisions]
(
	[eco_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_revisions_part_id]    ******/
CREATE NONCLUSTERED INDEX [IX_revisions_part_id] ON [dbo].[revisions]
(
	[part_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_revisions_one_current_per_part]    ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_revisions_one_current_per_part] ON [dbo].[revisions]
(
	[part_id] ASC
)
WHERE ([is_current]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[artifacts] ADD  CONSTRAINT [DF_artifacts_generated_at]  DEFAULT (sysutcdatetime()) FOR [generated_at]
GO
ALTER TABLE [dbo].[artifacts] ADD  CONSTRAINT [DF_artifacts_is_read_only]  DEFAULT ((1)) FOR [is_read_only]
GO
ALTER TABLE [dbo].[customers] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[customers] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[eco] ADD  CONSTRAINT [DF_eco_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[part_categories] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[part_categories] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[part_numbers] ADD  CONSTRAINT [DF_part_numbers_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[part_numbers] ADD  CONSTRAINT [DF_part_numbers_category]  DEFAULT ('--') FOR [category_code]
GO
ALTER TABLE [dbo].[part_numbers] ADD  CONSTRAINT [DF_part_numbers_document_type]  DEFAULT ('PART') FOR [document_type]
GO
ALTER TABLE [dbo].[projects] ADD  CONSTRAINT [DF_projects_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[projects] ADD  CONSTRAINT [DF_projects_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[projects] ADD  CONSTRAINT [DF_projects_project_seq]  DEFAULT ((0)) FOR [project_seq]
GO
ALTER TABLE [dbo].[revisions] ADD  CONSTRAINT [DF_revisions_revise_flag]  DEFAULT ((0)) FOR [revise_flag]
GO
ALTER TABLE [dbo].[revisions] ADD  CONSTRAINT [DF_revisions_is_current]  DEFAULT ((0)) FOR [is_current]
GO
ALTER TABLE [dbo].[revisions] ADD  CONSTRAINT [DF_revisions_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[revisions] ADD  CONSTRAINT [DF_revisions_lifecycle_state]  DEFAULT ('open') FOR [lifecycle_state]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[artifacts]  WITH CHECK ADD  CONSTRAINT [FK_artifacts_eco] FOREIGN KEY([eco_id])
REFERENCES [dbo].[eco] ([eco_id])
GO
ALTER TABLE [dbo].[artifacts] CHECK CONSTRAINT [FK_artifacts_eco]
GO
ALTER TABLE [dbo].[artifacts]  WITH CHECK ADD  CONSTRAINT [FK_artifacts_revision] FOREIGN KEY([revision_id])
REFERENCES [dbo].[revisions] ([revision_id])
GO
ALTER TABLE [dbo].[artifacts] CHECK CONSTRAINT [FK_artifacts_revision]
GO
ALTER TABLE [dbo].[eco]  WITH CHECK ADD  CONSTRAINT [FK_eco_project] FOREIGN KEY([project_id])
REFERENCES [dbo].[projects] ([project_id])
GO
ALTER TABLE [dbo].[eco] CHECK CONSTRAINT [FK_eco_project]
GO
ALTER TABLE [dbo].[part_numbers]  WITH CHECK ADD  CONSTRAINT [FK_part_numbers_category] FOREIGN KEY([category_code])
REFERENCES [dbo].[part_categories] ([category_code])
GO
ALTER TABLE [dbo].[part_numbers] CHECK CONSTRAINT [FK_part_numbers_category]
GO
ALTER TABLE [dbo].[part_numbers]  WITH CHECK ADD  CONSTRAINT [FK_part_numbers_current_revision] FOREIGN KEY([current_revision_id])
REFERENCES [dbo].[revisions] ([revision_id])
GO
ALTER TABLE [dbo].[part_numbers] CHECK CONSTRAINT [FK_part_numbers_current_revision]
GO
ALTER TABLE [dbo].[part_numbers]  WITH CHECK ADD  CONSTRAINT [FK_part_numbers_project] FOREIGN KEY([project_id])
REFERENCES [dbo].[projects] ([project_id])
GO
ALTER TABLE [dbo].[part_numbers] CHECK CONSTRAINT [FK_part_numbers_project]
GO
ALTER TABLE [dbo].[projects]  WITH CHECK ADD  CONSTRAINT [FK_projects_customers] FOREIGN KEY([customer_id])
REFERENCES [dbo].[customers] ([customer_id])
GO
ALTER TABLE [dbo].[projects] CHECK CONSTRAINT [FK_projects_customers]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [FK_revisions_eco] FOREIGN KEY([eco_id])
REFERENCES [dbo].[eco] ([eco_id])
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [FK_revisions_eco]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [FK_revisions_part] FOREIGN KEY([part_id])
REFERENCES [dbo].[part_numbers] ([part_id])
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [FK_revisions_part]
GO
ALTER TABLE [dbo].[artifacts]  WITH CHECK ADD  CONSTRAINT [CK_artifacts_artifact_type] CHECK  (([artifact_type]='other' OR [artifact_type]='dxf' OR [artifact_type]='stl' OR [artifact_type]='step' OR [artifact_type]='pdf'))
GO
ALTER TABLE [dbo].[artifacts] CHECK CONSTRAINT [CK_artifacts_artifact_type]
GO
ALTER TABLE [dbo].[eco]  WITH CHECK ADD  CONSTRAINT [CK_eco_release_level] CHECK  (([release_level]=(300) OR [release_level]=(200) OR [release_level]=(100)))
GO
ALTER TABLE [dbo].[eco] CHECK CONSTRAINT [CK_eco_release_level]
GO
ALTER TABLE [dbo].[eco]  WITH CHECK ADD  CONSTRAINT [CK_eco_state] CHECK  (([eco_state]='Cancelled' OR [eco_state]='Released' OR [eco_state]='Staged' OR [eco_state]='Development'))
GO
ALTER TABLE [dbo].[eco] CHECK CONSTRAINT [CK_eco_state]
GO
ALTER TABLE [dbo].[part_categories]  WITH CHECK ADD  CONSTRAINT [CK_part_categories_no_blank] CHECK  (([category_code]<>''))
GO
ALTER TABLE [dbo].[part_categories] CHECK CONSTRAINT [CK_part_categories_no_blank]
GO
ALTER TABLE [dbo].[part_numbers]  WITH CHECK ADD  CONSTRAINT [CK_part_numbers_document_type] CHECK  (([document_type]='DRAWING' OR [document_type]='ASSEMBLY' OR [document_type]='PART'))
GO
ALTER TABLE [dbo].[part_numbers] CHECK CONSTRAINT [CK_part_numbers_document_type]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [CK_revisions_current_consistency] CHECK  (([revision_state]='current' AND [lifecycle_state]='released' AND [is_current]=(1) OR ([revision_state]='obsolete' OR [revision_state]='superseded' OR [revision_state]='development') AND [is_current]=(0)))
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [CK_revisions_current_consistency]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [CK_revisions_lifecycle_state] CHECK  (([lifecycle_state]='cancelled' OR [lifecycle_state]='released' OR [lifecycle_state]='staged' OR [lifecycle_state]='open'))
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [CK_revisions_lifecycle_state]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [CK_revisions_revision_code_matches_family] CHECK  (([revision_family]=(100) AND ([revision_code]>=(101) AND [revision_code]<=(199)) OR [revision_family]=(200) AND ([revision_code]>=(201) AND [revision_code]<=(299)) OR [revision_family]=(300) AND ([revision_code]>=(301) AND [revision_code]<=(399))))
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [CK_revisions_revision_code_matches_family]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [CK_revisions_revision_family] CHECK  (([revision_family]=(300) OR [revision_family]=(200) OR [revision_family]=(100)))
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [CK_revisions_revision_family]
GO
ALTER TABLE [dbo].[revisions]  WITH CHECK ADD  CONSTRAINT [CK_revisions_revision_state] CHECK  (([revision_state]='obsolete' OR [revision_state]='superseded' OR [revision_state]='current' OR [revision_state]='development'))
GO
ALTER TABLE [dbo].[revisions] CHECK CONSTRAINT [CK_revisions_revision_state]
GO
/****** Object:  StoredProcedure [dbo].[usp_CreateArtifact]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_CreateArtifact]
(
    @part_number         NVARCHAR(50),
    @revision_code       INT,
    @artifact_type       NVARCHAR(20),
    @file_name           NVARCHAR(255),
    @file_path           NVARCHAR(500),
    @file_hash           NVARCHAR(128) = NULL,
    @source_file_guid    UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    DECLARE @part_id INT;
    DECLARE @revision_id INT;
    DECLARE @eco_id INT;
    DECLARE @revision_lifecycle_state NVARCHAR(30);
    DECLARE @eco_state NVARCHAR(30);
    DECLARE @artifact_number_int INT;

    ------------------------------------------------------------
    -- Validate artifact type
    ------------------------------------------------------------
    IF @artifact_type NOT IN ('pdf', 'step', 'stl', 'dxf', 'other')
        THROW 51000, 'Invalid artifact_type', 1;

    ------------------------------------------------------------
    -- Resolve part
    ------------------------------------------------------------
    SELECT @part_id = p.part_id
    FROM dbo.part_numbers p
    WHERE p.part_number = @part_number;

    IF @part_id IS NULL
        THROW 51001, 'Part not found', 1;

    ------------------------------------------------------------
    -- Resolve revision and governing ECO
    ------------------------------------------------------------
    SELECT
        @revision_id = r.revision_id,
        @eco_id = r.eco_id,
        @revision_lifecycle_state = r.lifecycle_state
    FROM dbo.revisions r
    WHERE r.part_id = @part_id
      AND r.revision_code = @revision_code;

    IF @revision_id IS NULL
        THROW 51002, 'Revision not found for specified part', 1;

    ------------------------------------------------------------
    -- Resolve ECO state
    ------------------------------------------------------------
    SELECT @eco_state = e.eco_state
    FROM dbo.eco e
    WHERE e.eco_id = @eco_id;

    IF @eco_state IS NULL
        THROW 51003, 'Governing ECO not found', 1;

    ------------------------------------------------------------
    -- Enforce released-only artifact creation
    ------------------------------------------------------------
    IF @revision_lifecycle_state <> 'released'
        THROW 51004, 'Artifacts may only be created from released revisions', 1;

    IF @eco_state <> 'released'
        THROW 51005, 'Artifacts may only be created when the governing ECO is released', 1;

    ------------------------------------------------------------
    -- Prevent exact duplicate path/type for same revision
    ------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.artifacts a
        WHERE a.revision_id = @revision_id
          AND a.artifact_type = @artifact_type
          AND a.file_path = @file_path
    )
        THROW 51006, 'Duplicate artifact for this revision/type/file_path', 1;

    ------------------------------------------------------------
    -- Get next artifact number
    ------------------------------------------------------------
    SET @artifact_number_int = NEXT VALUE FOR dbo.seq_artifact_number;

    ------------------------------------------------------------
    -- Insert artifact
    ------------------------------------------------------------
    INSERT INTO dbo.artifacts
    (
        artifact_number_int,
        revision_id,
        eco_id,
        artifact_type,
        file_name,
        file_path,
        file_hash,
        source_file_guid,
        generated_at,
        released_at,
        is_read_only
    )
    VALUES
    (
        @artifact_number_int,
        @revision_id,
        @eco_id,
        @artifact_type,
        @file_name,
        @file_path,
        @file_hash,
        @source_file_guid,
        SYSUTCDATETIME(),
        SYSUTCDATETIME(),
        1
    );

    ------------------------------------------------------------
    -- Return created artifact
    ------------------------------------------------------------
    SELECT
        a.artifact_id,
        a.artifact_number_int,
        a.artifact_number,
        p.part_number,
        r.revision_code,
        r.revision_family,
        e.eco_number,
        a.artifact_type,
        a.file_name,
        a.file_path,
        a.file_hash,
        a.source_file_guid,
        a.generated_at,
        a.released_at,
        a.is_read_only
    FROM dbo.artifacts a
    JOIN dbo.revisions r
        ON r.revision_id = a.revision_id
    JOIN dbo.part_numbers p
        ON p.part_id = r.part_id
    JOIN dbo.eco e
        ON e.eco_id = a.eco_id
    WHERE a.artifact_id = SCOPE_IDENTITY();

    COMMIT TRAN;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_CreateEco]    ******/

USE [ForgePLM]
GO
/****** Object:  StoredProcedure [dbo].[usp_CreateEco]    Script Date: 4/27/2026 3:46:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_CreateEco]
(
    @project_code     NVARCHAR(50),
    @eco_title        NVARCHAR(255),
    @eco_description  NVARCHAR(MAX) = NULL,
    @release_level    INT = 100
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @project_id INT;

    SELECT @project_id = project_id
    FROM dbo.projects
    WHERE project_code = @project_code
      AND is_active = 1;

    IF @project_id IS NULL
        THROW 50001, 'Invalid or inactive project_code', 1;

    IF @release_level NOT IN (100, 200, 300)
        THROW 50002, 'Invalid release level (must be 100, 200, or 300)', 1;

    DECLARE @eco_number_int INT = NEXT VALUE FOR dbo.seq_eco_number;

    INSERT INTO dbo.eco
    (
        eco_number_int,
        project_id,
        eco_title,
        eco_description,
        release_level,
        eco_state
    )
    VALUES
    (
        @eco_number_int,
        @project_id,
        @eco_title,
        @eco_description,
        @release_level,
        'Development'
    );

    SELECT
        eco_id,
        eco_number_int,
        eco_number,
        project_id,
        eco_title,
        eco_description,
        release_level,
        eco_state,
        created_at
    FROM dbo.eco
    WHERE eco_id = SCOPE_IDENTITY();
END;

/****** Object:  StoredProcedure [dbo].[usp_CreatePartNumber]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_CreatePartNumber]
(
    @project_code    NVARCHAR(50),
    @category_code   CHAR(2),
    @description     NVARCHAR(255) = NULL,
    @return_result   BIT = 1,
    @part_id INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @project_id INT;
    DECLARE @part_number_int INT;
    DECLARE @new_part_id INT;

    SELECT @project_id = project_id
    FROM dbo.projects
    WHERE project_code = @project_code
      AND is_active = 1;

    IF @project_id IS NULL
        THROW 50011, 'Invalid or inactive project_code', 1;

    IF @category_code IS NULL OR LTRIM(RTRIM(@category_code)) = ''
        THROW 52000, 'category_code is required', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.part_categories
        WHERE category_code = @category_code
          AND is_active = 1
    )
        THROW 52002, 'Invalid or inactive category_code', 1;

    SET @part_number_int = NEXT VALUE FOR dbo.seq_part_number;

    INSERT INTO dbo.part_numbers
    (
        part_number_int,
        project_id,
        category_code,
        description_current
    )
    VALUES
    (
        @part_number_int,
        @project_id,
        @category_code,
        @description
    );

    SET @new_part_id = SCOPE_IDENTITY();
    SET @part_id = SCOPE_IDENTITY();

    IF @return_result = 1
    BEGIN
        SELECT
            p.part_id,
            p.part_number_int,
            p.part_number,
            p.project_id,
            p.category_code,
            c.category_name,
            p.description_current,
            p.current_revision_id,
            p.created_at,
            p.retired_at
        FROM dbo.part_numbers p
        JOIN dbo.part_categories c
            ON c.category_code = p.category_code
        WHERE p.part_id = @new_part_id;
    END
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_CreatePartUnderEco]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_CreatePartUnderEco]
(
    @project_code   NVARCHAR(50),
    @eco_id         INT,
    @category_code  CHAR(2),
    @description    NVARCHAR(255) = NULL,
    @document_type  NVARCHAR(20) = 'PART'
)
AS
BEGIN TRY
BEGIN TRAN;
    SET NOCOUNT ON;

    DECLARE @project_id INT;
    DECLARE @eco_number NVARCHAR(50);
    DECLARE @revision_family INT;

    DECLARE @part_id INT;
    DECLARE @part_number_int INT;

    DECLARE @revision_id INT;
    DECLARE @revision_seq INT = 1;
    DECLARE @revision_code INT;

    SELECT
        @project_id = e.project_id,
        @eco_number = e.eco_number,
        @revision_family = e.release_level
    FROM dbo.eco e
    WHERE e.eco_id = @eco_id;

    IF @project_id IS NULL
        THROW 53000, 'Invalid eco_id', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.projects
        WHERE project_id = @project_id
          AND project_code = @project_code
          AND is_active = 1
    )
        THROW 53001, 'eco_id does not match project_code', 1;

    EXEC dbo.usp_CreatePartNumber
    @project_code = @project_code,
    @category_code = @category_code,
    @description = @description,
    @return_result = 0,
    @part_id = @part_id OUTPUT;

    SELECT
        @part_number_int = p.part_number_int
    FROM dbo.part_numbers p
    WHERE p.part_id = @part_id;

    IF @part_id IS NULL
        THROW 53002, 'Part creation failed.', 1;

    SET @revision_code = @revision_family + @revision_seq;

    INSERT INTO dbo.revisions
    (
        part_id,
        eco_id,
        revision_code,
        revision_family,
        revision_seq,
        part_description,
        revise_flag,
        revision_state,
        is_current,
        created_at,
        lifecycle_state
    )
    VALUES
    (
        @part_id,
        @eco_id,
        @revision_code,
        @revision_family,
        @revision_seq,
        @description,
        0,
        N'development',
        0,
        SYSDATETIME(),
        N'open'
    );

    SET @revision_id = SCOPE_IDENTITY();

    UPDATE dbo.part_numbers
    SET
        document_type = @document_type,
        description_current = @description
    WHERE part_id = @part_id;

    SELECT
        p.part_id,
        r.revision_id,
        p.category_code,
        p.part_number_int,
        r.revision_code,
        r.revision_family,
        r.revision_seq,
        r.revision_state,
        CONCAT(
            p.category_code, '-',
            RIGHT('0000000' + CAST(p.part_number_int AS varchar(7)), 7), '-',
            CAST(r.revision_code AS varchar(10))
        ) AS composite_code,
        COALESCE(r.part_description, '') AS part_description,
        e.eco_number,
        COALESCE(p.document_type, 'PART') AS document_type
    FROM dbo.part_numbers p
    INNER JOIN dbo.revisions r
        ON r.part_id = p.part_id
    INNER JOIN dbo.eco e
        ON e.eco_id = r.eco_id
    WHERE r.revision_id = @revision_id;
COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;
    THROW;
END CATCH

GO
/****** Object:  StoredProcedure [dbo].[usp_CreateRevision]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_CreateRevision]
(
    @part_number            NVARCHAR(50),
    @eco_number             NVARCHAR(50),
    @revision_description   NVARCHAR(255) = NULL,
    @part_description       NVARCHAR(255) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    DECLARE @part_id INT;
    DECLARE @eco_id INT;
    DECLARE @eco_state NVARCHAR(30);
    DECLARE @revision_family INT;
    DECLARE @next_revision_code INT;
    DECLARE @revision_id INT;

    ------------------------------------------------------------
    -- Resolve part
    ------------------------------------------------------------
    SELECT @part_id = p.part_id
    FROM dbo.part_numbers p
    WHERE p.part_number = @part_number;

    IF @part_id IS NULL
        THROW 50021, 'Part not found', 1;

    ------------------------------------------------------------
    -- Resolve ECO
    ------------------------------------------------------------
    SELECT
        @eco_id = e.eco_id,
        @eco_state = e.eco_state,
        @revision_family = e.release_level
    FROM dbo.eco e
    WHERE e.eco_number = @eco_number;

    IF @eco_id IS NULL
        THROW 50022, 'ECO not found', 1;

    IF @eco_state <> 'Development'
        THROW 50023, 'Revisions may only be created under an open ECO', 1;

    ------------------------------------------------------------
    -- One part may only be revised once under a given ECO
    ------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.revisions r
        WHERE r.part_id = @part_id
          AND r.eco_id = @eco_id
    )
        THROW 50024, 'This part already has a revision under the specified ECO', 1;

    ------------------------------------------------------------
    -- Determine next revision code within ECO family
    ------------------------------------------------------------
    SELECT @next_revision_code = MAX(r.revision_code)
    FROM dbo.revisions r
    WHERE r.part_id = @part_id
      AND r.revision_family = @revision_family;

    IF @next_revision_code IS NULL
        SET @next_revision_code = @revision_family + 1;
    ELSE
        SET @next_revision_code = @next_revision_code + 1;

    ------------------------------------------------------------
    -- Insert new in-development revision
    ------------------------------------------------------------
    INSERT INTO dbo.revisions
    (
        part_id,
        eco_id,
        revision_code,
        revision_family,
        revision_seq,
        revision_description,
        part_description,
        revision_state,
        is_current
    )
    VALUES
    (
        @part_id,
        @eco_id,
        @next_revision_code,
        @revision_family,
        @next_revision_code - @revision_family,
        @revision_description,
        @part_description,
        @eco_state,
        0
    );

    SET @revision_id = SCOPE_IDENTITY();

    ------------------------------------------------------------
    -- Return created revision
    ------------------------------------------------------------
    SELECT
        r.revision_id,
        p.part_number,
        r.revision_code,
        r.revision_family,
        r.revision_seq,
        r.revision_description,
        r.part_description,
        r.revision_state,
        r.is_current,
        e.eco_number,
        r.created_at
    FROM dbo.revisions r
    INNER JOIN dbo.part_numbers p
        ON p.part_id = r.part_id
    INNER JOIN dbo.eco e
        ON e.eco_id = r.eco_id
    WHERE r.revision_id = @revision_id;

    COMMIT TRAN;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_DeletePart]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_DeletePart]
    @part_id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.revisions WHERE part_id = @part_id;
    DELETE FROM dbo.part_numbers WHERE part_id = @part_id;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_ListArtifactsForRevision]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_ListArtifactsForRevision]
(
    @part_number     NVARCHAR(50),
    @revision_code   INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.artifact_id,
        a.artifact_number,
        a.artifact_type,
        a.file_name,
        a.file_path,
        a.file_hash,
        a.generated_at,
        a.released_at,
        a.is_read_only
    FROM dbo.artifacts a
    JOIN dbo.revisions r
        ON r.revision_id = a.revision_id
    JOIN dbo.part_numbers p
        ON p.part_id = r.part_id
    WHERE p.part_number = @part_number
      AND r.revision_code = @revision_code
    ORDER BY a.artifact_number_int;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_PromoteRevision]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_PromoteRevision]
(
    @part_number                  NVARCHAR(50),
    @eco_number                   NVARCHAR(50),
    @source_revision_code         INT = NULL,
    @revision_description         NVARCHAR(255) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    DECLARE @part_id INT;
    DECLARE @eco_id INT;
    DECLARE @eco_state NVARCHAR(30);
    DECLARE @to_revision_family INT;
    DECLARE @from_revision_family INT;

    DECLARE @source_revision_id INT;
    DECLARE @resolved_source_revision_code INT;
    DECLARE @source_part_description NVARCHAR(255);
    DECLARE @source_material NVARCHAR(100);
    DECLARE @source_unit NVARCHAR(50);
    DECLARE @source_unit_cost DECIMAL(18,4);
    DECLARE @source_unit_qty DECIMAL(18,4);
    DECLARE @source_markup DECIMAL(18,4);
    DECLARE @source_extended_price DECIMAL(18,4);
    DECLARE @source_sale_price DECIMAL(18,4);
    DECLARE @source_source NVARCHAR(255);
    DECLARE @source_vendor NVARCHAR(255);
    DECLARE @source_vendor_part_number NVARCHAR(100);
    DECLARE @source_file_guid UNIQUEIDENTIFIER;
    DECLARE @source_file_path NVARCHAR(500);

    DECLARE @next_target_revision_code INT;
    DECLARE @new_revision_id INT;

    SELECT @part_id = p.part_id
    FROM dbo.part_numbers p
    WHERE p.part_number = @part_number;

    IF @part_id IS NULL
        THROW 50103, 'Part not found', 1;

    SELECT
        @eco_id = e.eco_id,
        @eco_state = e.eco_state,
        @to_revision_family = e.release_level
    FROM dbo.eco e
    WHERE e.eco_number = @eco_number;

    IF @eco_id IS NULL
        THROW 50104, 'ECO not found', 1;

    IF @eco_state <> 'open'
        THROW 50106, 'Promotion may only be created under an open ECO', 1;

    IF @to_revision_family NOT IN (200, 300)
        THROW 50107, 'Promotion ECO must target family 200 or 300', 1;

    SET @from_revision_family = @to_revision_family - 100;

    IF EXISTS (
        SELECT 1
        FROM dbo.revisions r
        WHERE r.part_id = @part_id
          AND r.eco_id = @eco_id
    )
        THROW 50108, 'This part already has a revision under the specified ECO', 1;

    IF @source_revision_code IS NULL
    BEGIN
        SELECT TOP (1)
            @source_revision_id = r.revision_id,
            @resolved_source_revision_code = r.revision_code,
            @source_part_description = r.part_description,
            @source_material = r.material,
            @source_unit = r.unit,
            @source_unit_cost = r.unit_cost,
            @source_unit_qty = r.unit_qty,
            @source_markup = r.markup,
            @source_extended_price = r.extended_price,
            @source_sale_price = r.sale_price,
            @source_source = r.source,
            @source_vendor = r.vendor,
            @source_vendor_part_number = r.vendor_part_number,
            @source_file_guid = r.source_file_guid,
            @source_file_path = r.source_file_path
        FROM dbo.revisions r
        WHERE r.part_id = @part_id
          AND r.revision_family = @from_revision_family
          AND r.lifecycle_state = 'released'
        ORDER BY r.revision_code DESC;
    END
    ELSE
    BEGIN
        SELECT
            @source_revision_id = r.revision_id,
            @resolved_source_revision_code = r.revision_code,
            @source_part_description = r.part_description,
            @source_material = r.material,
            @source_unit = r.unit,
            @source_unit_cost = r.unit_cost,
            @source_unit_qty = r.unit_qty,
            @source_markup = r.markup,
            @source_extended_price = r.extended_price,
            @source_sale_price = r.sale_price,
            @source_source = r.source,
            @source_vendor = r.vendor,
            @source_vendor_part_number = r.vendor_part_number,
            @source_file_guid = r.source_file_guid,
            @source_file_path = r.source_file_path
        FROM dbo.revisions r
        WHERE r.part_id = @part_id
          AND r.revision_family = @from_revision_family
          AND r.revision_code = @source_revision_code
          AND r.lifecycle_state = 'released';
    END;

    IF @source_revision_id IS NULL
        THROW 50105, 'Released source revision not found in prior family', 1;

    SELECT @next_target_revision_code = MAX(r.revision_code)
    FROM dbo.revisions r
    WHERE r.part_id = @part_id
      AND r.revision_family = @to_revision_family;

    IF @next_target_revision_code IS NULL
        SET @next_target_revision_code = @to_revision_family + 1;
    ELSE
        SET @next_target_revision_code = @next_target_revision_code + 1;

    INSERT INTO dbo.revisions
    (
        part_id,
        eco_id,
        revision_code,
        revision_family,
        revision_seq,
        revision_description,
        part_description,
        material,
        unit,
        unit_cost,
        unit_qty,
        markup,
        extended_price,
        sale_price,
        source,
        vendor,
        vendor_part_number,
        lifecycle_state,
        revision_state,
        release_state,
        is_current,
        source_file_guid,
        source_file_path
    )
    VALUES
    (
        @part_id,
        @eco_id,
        @next_target_revision_code,
        @to_revision_family,
        @next_target_revision_code - @to_revision_family,
        COALESCE(
            @revision_description,
            CONCAT('Promoted from ', @resolved_source_revision_code, ' to ', @next_target_revision_code)
        ),
        @source_part_description,
        @source_material,
        @source_unit,
        @source_unit_cost,
        @source_unit_qty,
        @source_markup,
        @source_extended_price,
        @source_sale_price,
        @source_source,
        @source_vendor,
        @source_vendor_part_number,
        'open',
        'superseded',
        'unreleased',
        0,
        @source_file_guid,
        @source_file_path
    );

    SET @new_revision_id = SCOPE_IDENTITY();

    SELECT
        r.revision_id,
        p.part_number,
        @resolved_source_revision_code AS source_revision_code,
        r.revision_code AS promoted_revision_code,
        r.revision_family,
        r.revision_seq,
        r.revision_description,
        r.part_description,
        r.lifecycle_state,
        r.revision_state,
        r.release_state,
        r.is_current,
        e.eco_number,
        r.created_at
    FROM dbo.revisions r
    INNER JOIN dbo.part_numbers p
        ON p.part_id = r.part_id
    INNER JOIN dbo.eco e
        ON e.eco_id = r.eco_id
    WHERE r.revision_id = @new_revision_id;

    COMMIT TRAN;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_ReleaseEco]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_ReleaseEco]
(
    @eco_number                  NVARCHAR(50),
    @part_number                 NVARCHAR(50),
    @previous_revision_state     NVARCHAR(20) = NULL   -- superseded / obsolete
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    DECLARE @eco_id INT;
    DECLARE @eco_state NVARCHAR(30);
    DECLARE @eco_family INT;
    DECLARE @part_id INT;
    DECLARE @new_revision_id INT;
    DECLARE @previous_current_revision_id INT;

    ------------------------------------------------------------
    -- Resolve ECO
    ------------------------------------------------------------
    SELECT
        @eco_id = e.eco_id,
        @eco_state = e.eco_state,
        @eco_family = e.release_level
    FROM dbo.eco e
    WHERE e.eco_number = @eco_number;

    IF @eco_id IS NULL
        THROW 50300, 'ECO not found', 1;

    IF @eco_state NOT IN ('open', 'staged')
        THROW 50301, 'Only open or staged ECOs may be released', 1;

    ------------------------------------------------------------
    -- Resolve part
    ------------------------------------------------------------
    SELECT @part_id = p.part_id
    FROM dbo.part_numbers p
    WHERE p.part_number = @part_number;

    IF @part_id IS NULL
        THROW 50302, 'Part not found', 1;

    ------------------------------------------------------------
    -- Resolve new revision under ECO for this part
    ------------------------------------------------------------
    SELECT @new_revision_id = r.revision_id
    FROM dbo.revisions r
    WHERE r.eco_id = @eco_id
      AND r.part_id = @part_id;

    IF @new_revision_id IS NULL
        THROW 50303, 'Specified part has no revision under the ECO', 1;

    ------------------------------------------------------------
    -- Ensure ECO family matches revision family
    ------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.revisions r
        WHERE r.revision_id = @new_revision_id
          AND r.revision_family <> @eco_family
    )
        THROW 50304, 'Revision family does not match ECO release level', 1;

    ------------------------------------------------------------
    -- Find existing current revision, excluding the new one
    ------------------------------------------------------------
    SELECT @previous_current_revision_id = r.revision_id
    FROM dbo.revisions r
    WHERE r.part_id = @part_id
      AND r.is_current = 1
      AND r.revision_id <> @new_revision_id;

    IF @previous_current_revision_id IS NOT NULL
       AND @previous_revision_state NOT IN ('superseded', 'obsolete')
        THROW 50305, 'Must specify previous_revision_state as superseded or obsolete', 1;

    ------------------------------------------------------------
    -- Demote previous current revision if applicable
    ------------------------------------------------------------
    IF @previous_current_revision_id IS NOT NULL
    BEGIN
        UPDATE dbo.revisions
        SET
            is_current = 0,
            revision_state = @previous_revision_state
        WHERE revision_id = @previous_current_revision_id;
    END;

    ------------------------------------------------------------
    -- Promote new revision to released/current
    ------------------------------------------------------------
    UPDATE dbo.revisions
    SET
        lifecycle_state = 'released',
        revision_state = 'current',
        release_state = 'released',
        is_current = 1
    WHERE revision_id = @new_revision_id;

    ------------------------------------------------------------
    -- Update part current pointer
    ------------------------------------------------------------
    UPDATE dbo.part_numbers
    SET current_revision_id = @new_revision_id
    WHERE part_id = @part_id;

    ------------------------------------------------------------
    -- If all revisions under the ECO are now released, mark ECO released
    ------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.revisions r
        WHERE r.eco_id = @eco_id
          AND r.lifecycle_state <> 'released'
    )
    BEGIN
        UPDATE dbo.eco
        SET eco_state = 'released'
        WHERE eco_id = @eco_id;
    END;

    ------------------------------------------------------------
    -- Return result
    ------------------------------------------------------------
    SELECT
        e.eco_number,
        e.eco_state,
        p.part_number,
        r.revision_code,
        r.revision_family,
        r.lifecycle_state,
        r.revision_state,
        r.release_state,
        r.is_current
    FROM dbo.revisions r
    JOIN dbo.part_numbers p
        ON p.part_id = r.part_id
    JOIN dbo.eco e
        ON e.eco_id = r.eco_id
    WHERE r.revision_id = @new_revision_id;

    COMMIT TRAN;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_SetEcoState]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_SetEcoState]
(
    @eco_number    NVARCHAR(50),
    @new_state     NVARCHAR(30)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @new_state NOT IN ('open', 'staged', 'cancelled')
        THROW 50200, 'Invalid ECO state for this procedure', 1;

    UPDATE dbo.eco
    SET eco_state = @new_state
    WHERE eco_number = @eco_number;

    IF @@ROWCOUNT = 0
        THROW 50202, 'ECO not found', 1;

    SELECT *
    FROM dbo.eco
    WHERE eco_number = @eco_number;
END;
GO


USE [ForgePLM]
GO
/****** Object:  StoredProcedure [dbo].[usp_CreateCustomer]    Script Date: 4/27/2026 12:24:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[usp_CreateCustomer]
    @customer_code NVARCHAR(25),
    @customer_name NVARCHAR(200),
    @contact_name NVARCHAR(200) = NULL,
    @contact_email NVARCHAR(200) = NULL,
    @contact_phone NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.customers
        WHERE customer_code = @customer_code
    )
    BEGIN
        THROW 50010, 'Customer code already exists.', 1;
    END;

    INSERT INTO dbo.customers
    (
        customer_code,
        customer_name,
        contact_name,
        contact_email,
        contact_phone,
        is_active,
        created_at
    )
    VALUES
    (
        @customer_code,
        @customer_name,
        @contact_name,
        @contact_email,
        @contact_phone,
        1,
        SYSUTCDATETIME()
    );
END;

USE [ForgePLM]
GO
/****** Object:  StoredProcedure [dbo].[usp_CreateProject]    Script Date: 4/27/2026 2:54:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_CreateProject]
    @customer_id INT,
    @project_code NVARCHAR(25), -- base code (e.g. VRAD0)
    @project_name NVARCHAR(200),
    @project_description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate customer
    IF NOT EXISTS (
        SELECT 1 FROM dbo.customers
        WHERE customer_id = @customer_id
          AND is_active = 1
    )
    BEGIN
        THROW 50020, 'Invalid or inactive customer.', 1;
    END;

    DECLARE @next_project_seq INT;
    DECLARE @final_project_code NVARCHAR(50);

    -- Get next sequence per customer (LOCKED)
    SELECT @next_project_seq = ISNULL(MAX(project_seq), 0) + 1
    FROM dbo.projects WITH (UPDLOCK, HOLDLOCK)
    WHERE customer_id = @customer_id;
    
    IF @project_code LIKE '%-%'
    BEGIN
        SET @project_code = LEFT(@project_code, CHARINDEX('-', @project_code) - 1);
    END;

    IF LEN(@project_code) < 3
    BEGIN
        THROW 50022, 'Invalid base project code after normalization.', 1;
    END;

    -- Build formatted project code (e.g. VRAD0-00001)
    SET @final_project_code = 
        @project_code + '-' + RIGHT('00000' + CAST(@next_project_seq AS VARCHAR(5)), 5);

    -- Ensure final code is unique
    IF EXISTS (
        SELECT 1 FROM dbo.projects
        WHERE project_code = @final_project_code
    )
    BEGIN
        THROW 50021, 'Generated project code already exists.', 1;
    END;

    -- Insert
    INSERT INTO dbo.projects
    (
        customer_id,
        project_seq,
        project_code,
        project_name,
        project_description,
        is_active,
        created_at
    )
    VALUES
    (
        @customer_id,
        @next_project_seq,
        @final_project_code,
        @project_name,
        @project_description,
        1,
        SYSUTCDATETIME()
    );

    -- Return inserted record
    SELECT
        project_id,
        customer_id,
        project_seq,
        project_code,
        project_name,
        project_description,
        is_active
    FROM dbo.projects
    WHERE project_code = @final_project_code;
END;
