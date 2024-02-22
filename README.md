# pdftool

a tools for pdf has some functions like merge images to pdf or import a pdf to images.

this tool use library of  [dlemstra/Magick.NET](https://github.com/dlemstra/Magick.NET)



### **Usage**

```bash
pdftool.exe  [command] [options]
```



### **Command**

this tool has two command,merge and export

**Merge**

merge all images in the directory to a pdf file
```bash
pdftool.exe merge -d=d:\test -name=123
```

| name   | alias | required | description                 |
| ------ | ----- | -------- | --------------------------- |
| --dir  | -d    | true     | the directory of the images |
| --name | -n    | true     | the name of pdf             |



**Export**

export pages in pdf to images
```bash
pdftool.exe export -d=d:\test -s=d:\test\123.pdf
```

|  name   | alias | required | description                 |
|  ------ | ----- | -------- | --------------------------- |
|  --dir  | -d    | true     | the directory of the images to save |
|  --src  | -s    | true     | the path of pdf             |
|  --start|       | false    | the page no where to export from |
| --number|       | false    | how many pages will export from start |

