# Spreadsheet
##Mimics basic functionality for Microsoft Excel in WinForms using C# for CPTS 321: C# development

##Main features:
  ###Utilizes INotifyProperty to seperate UI and logic engines
  ###Expression and equation handling
      ####Ex. "=A2 * (B1 + 100)" would evaluate properly
  ###Undo/Redo:
      ####Actions such as changing background color and cell text will be pushed onto an undo stack.
      Actions undone will be pushed onto a redo stack
  Bad/circular reference:
      Expressions will be evaluated before being calculated to make sure that there's no bad references "=ABEFD * 2 + 2",
      or circular references A2: "= A1 * B2 + A2"
