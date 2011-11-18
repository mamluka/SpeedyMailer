using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;


namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailCSVParserTests:AutoMapperAndFixtureBase
    {

       
        public MemoryStream GetDataStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);


            writer.WriteLine("stock #,shape,weight,color,clarity,rapnet price,rap-price,rap%,lab,certificate #,measurements,depth %,table %,girdle,culet,polish,sym,fluorescence intensity,comment,rapcode,pair,pairsep,Cut Grade,Crown Angle,Crown Height,Pavilion Angle,Pavilion Depth,Fancy Color,Fancy Color Intensity,Certificate filename");
            writer.WriteLine("DY    -029   ,BR    ,1.16,H        ,SI2      ,2867,6100,-0.53,EGL IL  ,            , 4.30* 5.24* 2.32,65.8,58.5,               ,          ,VG        ,VG        ,NIL                 , , ,             , ,VG        ,0,0,0,0,         ,          ,");
            writer.Flush();
            stream.Position = 0;

            return stream;
        }


    }
}