# How well does Smart.Format perform?

Even with all its features, **SmartFormat** is efficient and has amazing performance.  It has several features that make it as fast as possible.

* **Fast Parser**: The parsing engine is written from scratch to maximize speed and efficiency.

* **Extension System**: You can easily select the set of "extensions" that are used in formatting, and unused extensions can be removed.  Multiple formatters can be created, each with a specific set of extensions.  For example, if you only use the plural formatting extension, you can remove the reflection extension to increase performance.

* **FormatWithCache**: **SmartFormat** even has a `FormatWithCache` method that caches parsing results, resulting in the fastest performance possible!  Extensions can also take advantage of the cache if they need to.

* **IOutput**: **SmartFormat** uses a custom interface for output, meaning that the results could be directly output to any stream without having to build intermediate strings.

# Performance Comparisons

## Compared to `String.Format`

Due to its ability to cache and output directly to streams, in some scenarios `Smart.Format` can actually outperform `String.Format`!

For example, a lengthy email template would benefit greatly from the `FormatWithCache` method and could easily outperform `String.Format` by about 25%.

Even without caching, the basic `Smart.Format` method takes only about 50% longer than `String.Format` on the same format string.

## Compared to other "Named Placeholder" methods

There is a collection of other "Named Placeholder" methods that you can read about on [[Phil Haack's Blog|http://haacked.com/archive/2009/01/14/named-formats-redux.aspx]].  These implementations were created by some people you might have heard of, including [[Phil Haack|http://www.haacked.com/]], [[Scott Hanselman|http://www.hanselman.com/blog/]], [[James Newton-King|http://james.newtonking.com/]], [[Oskar Austegard|http://mo.notono.us/]], and Henri Wiechers.

You can [[download the source code|http://research.microsoft.com/en-us/projects/pex/namedstringformatsolution.zip]] from [[Phil Haack's Blog|http://haacked.com/archive/2009/01/14/named-formats-redux.aspx]] or now even as a sample from [[Microsoft's Pex website|http://research.microsoft.com/en-us/downloads/d2279651-851f-4d7a-bf05-16fd7eb26559/default.aspx]].

While these methods all have good performance, `Smart.Format` and `Smart.FormatWithCache` are actually the fastest of them all, beating even Henri's method by a small fraction!

Modify the performance tests as follows:
<pre>                MeasureFormatTime("Hanselformat", () => format.HanselFormat(o));
                MeasureFormatTime("OskarFormat", () => format.OskarFormat(o));
                MeasureFormatTime("JamesFormat", () => format.JamesFormat(o));
                MeasureFormatTime("HenriFormat", () => format.HenriFormat(o));
                MeasureFormatTime("HaackFormat", () => format.HaackFormat(o));
                // New Smart.Format tests:
                MeasureFormatTime("SmartFormat", () => format.SmartFormat(o));
                SmartFormat.Core.FormatCache cache = null;
                MeasureFormatTime("CacheFormat", () => format.SmartFormat(ref cache, o));
</pre>

Here are my results.  They represent the average of running the test 5 times:

<pre>HanselFormat took 0.26340 ms
OskarFormat took 0.32200 ms
JamesFormat took 0.19280 ms
HenriFormat took 0.13960 ms   - 3rd fastest
HaackFormat took 0.15880 ms
SmartFormat took 0.13700 ms   - 2nd fastest
CacheFormat took 0.12780 ms   - fastest!
</pre>

As you can see, the performance of `Smart.Format` is wonderful and should compete with any other implementation.
