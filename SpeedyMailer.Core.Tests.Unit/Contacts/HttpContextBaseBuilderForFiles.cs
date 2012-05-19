using System.IO;
using System.Web;
using Rhino.Mocks;

namespace SpeedyMailer.Core.Tests.Unit.Contacts
{
    public class HttpContextBaseBuilderForFiles
    {
        private readonly HttpContextBase httpContext;
        private HttpPostedFileBase file;
        private int fileCount;
        private string fileName;
        private Stream fileStream;
        private HttpRequestBase httpRequest;

        public HttpContextBaseBuilderForFiles()
        {
            fileCount = 1;
            fileName = "contacts.csv";
            fileStream = new MemoryStream();
            httpContext = MockRepository.GenerateStub<HttpContextBase>();
            httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
            Files = MockRepository.GenerateStub<HttpFileCollectionBase>();
            file = MockRepository.GenerateStub<HttpPostedFileBase>();
        }

        public HttpFileCollectionBase Files { get; set; }


        public HttpContextBaseBuilderForFiles WithFileCount(int fileCount)
        {
            this.fileCount = fileCount;
            return this;
        }

        public HttpContextBaseBuilderForFiles WithFileStream(Stream stream)
        {
            fileStream = stream;
            return this;
        }

        public HttpContextBaseBuilderForFiles WithFileName(string fileName)
        {
            this.fileName = fileName;
            return this;
        }

        public HttpContextBase Build()
        {
            httpContext.Stub(x => x.Request).Return(httpRequest);
            BuildFiles();
            httpRequest.Stub(x => x.Files).Return(Files);

            return httpContext;
        }

        public HttpContextBaseBuilderForFiles BuildFiles()
        {
            Files.Stub(x => x.Count).Return(fileCount);

            Files.Stub(x => x[0]).Return(file);

            file.Stub(x => x.InputStream).Return(fileStream);
            file.Stub(x => x.FileName).Return(fileName);

            return this;
        }


        public HttpContextBaseBuilderForFiles Replace(HttpRequestBase replacingHttpRequest)
        {
            httpRequest = replacingHttpRequest;
            return this;
        }

        public void Replace(HttpPostedFileBase replacingHttpPostedFile)
        {
            file = replacingHttpPostedFile;
        }
    }
}