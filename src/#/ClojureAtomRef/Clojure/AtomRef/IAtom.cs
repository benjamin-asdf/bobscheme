/**
 *   Copyright (c) Rich Hickey. All rights reserved.
 *   The use and distribution terms for this software are covered by the
 *   Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php)
 *   which can be found in the file epl-v10.html at the root of this distribution.
 *   By using this software in any fashion, you are agreeing to be bound by
 * 	 the terms of this license.
 *   You must not remove this notice, or any other, from this software.
 **/

/**
 *   Author: David Miller
 **/

// Benjamin: Selected a subset and added generic param
// reasoning is a generic is less surprising in a csharp code base
// Changed namespace


using System;
using System.Diagnostics.CodeAnalysis;

namespace clojure.AtomRef;

public interface IAtom<T> {
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ClojureJVM name match")]
    T swap(Func<T, T> f);


    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ClojureJVM name match")]
    bool compareAndSet(T oldv, T newv);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ClojureJVM name match")]
    T reset(T newval);
}
