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


// Benjamin:
// generic instead of dynamic
// removed Aref, removed watches, validators, and meta data
// Changed namespace

using System;

namespace clojure.AtomRef;

/// <summary>
///     Provides spin-loop synchronized access to a value.  One of the reference types.
/// </summary>
public class Atom<T> : IAtom<T> where T : class {
        #region Data

    /// <summary>
    ///     The atom's value.
    /// </summary>
    private readonly AtomicReference<T> _state;

        #endregion

        #region Ctors and factory methods

    /// <summary>
    ///     Construct an atom with given intiial value.
    /// </summary>
    /// <param name="state">The initial value</param>
    public Atom(T state) {
        _state = new AtomicReference<T>(state);
    }

        #endregion

        #region IDeref methods

    /// <summary>
    ///     Gets the (immutable) value the reference is holding.
    /// </summary>
    /// <returns>The value</returns>
    public T deref() {
        return _state.Get();
    }

        #endregion

        #region State manipulation

    /// <summary>
    ///     Compute and set a new value.  Spin loop for coordination.
    /// </summary>
    /// <param name="f">The function to apply to the current state.</param>
    /// <returns>The new value.</returns>
    public T swap(Func<T, T> f) {
        for (;;) {
            var v = deref();
            var newv = f.Invoke(v);
            if (_state.CompareAndSet(v, newv)) return newv;
        }
    }


    /// <summary>
    ///     Compare/exchange the value.
    /// </summary>
    /// <param name="oldv">The expected value.</param>
    /// <param name="newv">The new value.</param>
    /// <returns>
    ///     <value>true</value>
    ///     if the value was set;
    ///     <value>false</value>
    ///     otherwise.
    /// </returns>
    public bool compareAndSet(T oldv, T newv) {
        var ret = _state.CompareAndSet(oldv, newv);
        return ret;
    }


    /// <summary>
    ///     Set the value.
    /// </summary>
    /// <param name="newval">The new value.</param>
    /// <returns>The new value.</returns>
    public T reset(T newval) {
        _state.Set(newval);
        return newval;
    }

        #endregion
}
