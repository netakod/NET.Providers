
Telnet Class Library for .NET 2.0 (Version 0.2)
Copyright (c) 2007 David Pinch * http://www.thoughtproject.com * mailto:dave@thoughtproject.com

INTRODUCTION

    This is the beginning of a .NET class library for implementing the Telnet protocol.
    The current code is not finished and still needs much work.  I am sharing the code now
    in hopes that other hobbyists will be interested in helping.  I do not recommend
    using the code for production systems.  All code is under the LGPL license to allow
    use with commercial applications.
  

KNOWN ISSUES

    The protocol options will not be detected by the TelnetClient class until
    a read operation occurs.  This is because the class does not implement
    background buffers; instead it looks for protocol options as it reads data
    from the underlying socket.  The next version will implement background,
    asynchronous buffering.

    The client requires a blocking socket.  This will also be fixed once
    asynchronous background buffering is implemented.
    

SUPPORT AND DOWNLOADS

    http://www.codeplex.com/telnet
    http://www.thoughtproject.com/Libraries/Telnet/
  
    
AUTHORS/CONTRIBUTORS

    David Pinch
    http://www.linkedin.com/in/davepinch
  
  
LICENSE AND COPYRIGHTS

  Telnet Class Library for .NET
  Copyright (C) 2007 David Pinch (dave@thoughtproject.com)
  
  This library is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 2.1 of the License, or (at your option) any later version.

  This library is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public
  License along with this library; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
  