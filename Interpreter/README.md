# Brainfuck.Interpreter

brainfuck dotnet tool.

## install (pre-release)

```bash
dotnet tool install -g dotnet-brainfuck --version 0.1.1-preview-1
```

## update (pre-release)

```bash
dotnet tool update -g dotnet-brainfuck --version 0.1.1-preview-1
```

## usage

```bash
dotnet-brainfuck "++++++[>++++++++<-]++++++++++[>.+<-]"
# 0123456789

dotnet-brainfuck parse "++>--"
```
