﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ClipConverter.Models;
public class Blob
{
    public string? ContentType;
    public string? Name;
    public Stream? Content;
}
