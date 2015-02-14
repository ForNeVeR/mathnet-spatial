﻿// --------------------------------------------------------------------------------------
// Builds the documentation from `.fsx` and `.md` files in the 'docs/content' directory
// (the generated documentation is stored in the 'docs/output' directory)
// --------------------------------------------------------------------------------------

// Binaries that have XML documentation (in a corresponding generated XML file)
let referenceBinaries = [ "MathNet.Spatial.dll"; "MathNet.Numerics.dll" ]
// Web site location for the generated documentation
let website = "http://spatial.mathdotnet.com"
let githubLink = "http://github.com/mathnet/mathnet-spatial"

// Specify more information about your project
let info =
  [ "project-name", "Math.NET Spatial"
    "project-author", "Christoph Ruegg, Johan Larsson"
    "project-summary", "Math.NET Spatial. .Net 4, .Net 3.5, SL5, Win8, WP8, PCL 47 and 136, Mono, Xamarin Android/iOS."
    "project-github", githubLink
    "project-nuget", "http://nuget.com/packages/MathNet.Spatial" ]

// --------------------------------------------------------------------------------------
// For typical project, no changes are needed below
// --------------------------------------------------------------------------------------

#load "../../packages/FSharp.Formatting/FSharp.Formatting.fsx"
#r "../../packages/FAKE/tools/NuGet.Core.dll"
#r "../../packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO
open Fake.FileHelper
open FSharp.Literate
open FSharp.MetadataFormat

// When called from 'build.fsx', use the public project URL as <root>
// otherwise, use the current 'output' directory.
#if RELEASE
let root = website
#else
let root = "file://" + (__SOURCE_DIRECTORY__ @@ "../../out/docs")
#endif

// Paths with template/source/output locations
let top        = __SOURCE_DIRECTORY__ @@ "../../"
let bin        = __SOURCE_DIRECTORY__ @@ "../../out/lib/Net40"
let content    = __SOURCE_DIRECTORY__ @@ "../content"
let output     = __SOURCE_DIRECTORY__ @@ "../../out/docs"
let files      = __SOURCE_DIRECTORY__ @@ "../files"
let templates  = __SOURCE_DIRECTORY__ @@ "templates"
let formatting = __SOURCE_DIRECTORY__ @@ "../../packages/FSharp.Formatting/"
let docTemplate = formatting @@ "templates/docpage.cshtml"

// Where to look for *.csproj templates (in this order)
let layoutRoots =
    [ templates
      formatting @@ "templates"
      formatting @@ "templates/reference" ]

// Copy static files and CSS + JS from F# Formatting
let copyFiles() =
  CopyRecursive files output true |> Log "Copying file: "
  ensureDirectory (output @@ "content")
  CopyRecursive (formatting @@ "styles") (output @@ "content") true
    |> Log "Copying styles and scripts: "

// Build API reference from XML comments
let buildReference() =
  CleanDir (output @@ "reference")
  let binaries =
    referenceBinaries
    |> List.map (fun lib-> bin @@ lib)
  MetadataFormat.Generate
    ( binaries, output @@ "reference", layoutRoots,
      parameters = ("root", root)::info,
      sourceRepo = githubLink @@ "tree/master",
      sourceFolder = __SOURCE_DIRECTORY__ @@ ".." @@ "..",
      publicOnly = true, libDirs = [bin] )

// Build documentation from `fsx` and `md` files in `docs/content`
let buildDocumentation() =
  let subdirs = Directory.EnumerateDirectories(content, "*", SearchOption.AllDirectories)
  for dir in Seq.append [content] subdirs do
    let sub = if dir.Length > content.Length then dir.Substring(content.Length + 1) else "."
    Literate.ProcessDirectory
      ( dir, docTemplate, output @@ sub, replacements = ("root", root)::info,
        layoutRoots = layoutRoots,
        references = false,
        lineNumbers = true,
        generateAnchors = true )

// Generate
copyFiles()
buildDocumentation()
