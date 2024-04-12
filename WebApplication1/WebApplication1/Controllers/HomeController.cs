using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string pattern)
        {
            List<SearchResultLine> result;
            using (TestTableJoin11Entities db = new TestTableJoin11Entities())
            {
                result = db.Students
                    .Join(
                        db.StudentsCourses,
                        s => s.Id,
                        sc => sc.Stud_Id,
                        (s, sc) => new
                        {
                            Name = s.Name,
                            Surname = s.Surname,
                            id_s_c = sc.Id,
                            id_c = sc.Course_Id
                        }
                   ).Join(
                        db.Courses,
                        sc => sc.id_c,
                        c => c.Id,
                        (sc, c) => new SearchResultLine()
                        {
                            Name = sc.Name,
                            Surname = sc.Surname,
                            CourseName = c.Name
                        }
                    ).ToList();
            }
            if (pattern == null)
            {
                ViewBag.SearchData = result;
                return View();
            }
            else
            {
                result = result.Where((p) => p.Name.Contains(pattern)).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public FileStreamResult GetWord()
        {
            string[,] data = new string[3, 5];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 5; j++)
                    data[i, j] = (i + j).ToString();
            MemoryStream memoryStream = GenerateWord(data);
            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = "demo.docx"
            };
        }

        private MemoryStream GenerateWord(string[,] data)
        {
            MemoryStream mStream = new MemoryStream();
            // Создаем документ
            using (WordprocessingDocument document =
                WordprocessingDocument.Create(mStream, WordprocessingDocumentType.Document, true))
            {
                // Добавляется главная часть документа. 
                MainDocumentPart mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                // Создаем таблицу. 
                Table table = new Table();
                body.AppendChild(table);

                // Устанавливаем свойства таблицы(границы и размер).
                TableProperties props = new TableProperties(
                    new TableBorders(
                    new TopBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new BottomBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new LeftBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new RightBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new InsideHorizontalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new InsideVerticalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    }));

                // Назначаем свойства props объекту table
                table.AppendChild<TableProperties>(props);

                // Заполняем ячейки таблицы.
                for (var i = 0; i <= data.GetUpperBound(0); i++)
                {
                    var tr = new TableRow();
                    for (var j = 0; j <= data.GetUpperBound(1); j++)
                    {
                        var tc = new TableCell();
                        tc.Append(new Paragraph(new Run(new Text(data[i, j]))));

                        // размер колонок определяется автоматически.
                        tc.Append(new TableCellProperties(
                            new TableCellWidth { Type = TableWidthUnitValues.Auto }));

                        tr.Append(tc);
                    }
                    table.Append(tr);
                }

                mainPart.Document.Save();
            }
            mStream.Position = 0;
            return mStream;
        }
    }
}