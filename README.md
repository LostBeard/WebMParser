# WebMParser

WebMParser is a WebM parser written entirely in C#. 

The initial goal of this library is to allow adding a duration to WebM video files recorded using [MediaRecorder](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder) in a web browser.  

The demo project included with the library is a .Net core 8 console app that currently just allows testing the library.  

To fix the duration in a WebM file, WebM parser reads the Timecode information from Clusters and SimpleBlocks and adds a Segment > Info > Duration element with the new duration.

