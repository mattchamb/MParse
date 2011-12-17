using System;
using System.Collections.Generic;


namespace MParse.GrammarProviders.FileProvider
{
    internal class Lexer
    {

        /* #line 2 "rt.cs" */

        private static readonly sbyte[] _ragelmachine_actions = new sbyte[]
                                                                    {
                                                                        0, 1, 0, 1, 1, 1, 2, 1,
                                                                        3, 1, 4, 1, 5, 1, 6, 1,
                                                                        7, 1, 8, 1, 9
                                                                    };

        private static readonly sbyte[] _ragelmachine_key_offsets = new sbyte[]
                                                                        {
                                                                            0, 0, 1, 2, 3, 4, 5, 18
                                                                        };

        private static readonly char[] _ragelmachine_trans_keys = new char[]
                                                                      {
                                                                          '\u0023', '\u0025', '\u003a', '\u003d',
                                                                          '\u0040', '\u0020', '\u0023', '\u0025',
                                                                          '\u003a', '\u003b', '\u003d', '\u0040',
                                                                          '\u0009', '\u000d', '\u0041', '\u005a',
                                                                          '\u0061', '\u007a', '\u005f', '\u0030',
                                                                          '\u0039', '\u0041', '\u005a', '\u0061',
                                                                          '\u007a', (char) 0
                                                                      };

        private static readonly sbyte[] _ragelmachine_single_lengths = new sbyte[]
                                                                           {
                                                                               0, 1, 1, 1, 1, 1, 7, 1
                                                                           };

        private static readonly sbyte[] _ragelmachine_range_lengths = new sbyte[]
                                                                          {
                                                                              0, 0, 0, 0, 0, 0, 3, 3
                                                                          };

        private static readonly sbyte[] _ragelmachine_index_offsets = new sbyte[]
                                                                          {
                                                                              0, 0, 2, 4, 6, 8, 10, 21
                                                                          };

        private static readonly sbyte[] _ragelmachine_trans_targs = new sbyte[]
                                                                        {
                                                                            6, 0, 6, 0, 4, 0, 6, 0,
                                                                            6, 0, 6, 1, 2, 3, 6, 6,
                                                                            5, 6, 7, 7, 0, 7, 7, 7,
                                                                            7, 6, 6, 0
                                                                        };

        private static readonly sbyte[] _ragelmachine_trans_actions = new sbyte[]
                                                                          {
                                                                              9, 0, 7, 0, 0, 0, 15, 0,
                                                                              5, 0, 17, 0, 0, 0, 11, 13,
                                                                              0, 17, 0, 0, 0, 0, 0, 0,
                                                                              0, 19, 19, 0
                                                                          };

        private static readonly sbyte[] _ragelmachine_to_state_actions = new sbyte[]
                                                                             {
                                                                                 0, 0, 0, 0, 0, 0, 1, 0
                                                                             };

        private static readonly sbyte[] _ragelmachine_from_state_actions = new sbyte[]
                                                                               {
                                                                                   0, 0, 0, 0, 0, 0, 3, 0
                                                                               };

        private static readonly sbyte[] _ragelmachine_eof_trans = new sbyte[]
                                                                      {
                                                                          0, 0, 0, 0, 0, 0, 0, 27
                                                                      };

        private const int ragelmachine_start = 6;
        private const int ragelmachine_first_final = 6;
        private const int ragelmachine_error = 0;

        private const int ragelmachine_en_main = 6;


        /* #line 33 "rt.rl" */

        public static IEnumerable<_Terminal> Run(string data)
        {
            int cs;
            int p = 0;
            int eof;
            int act;
            int ts, te;
            int pe = eof = data.Length;

            // init

            /* #line 63 "rt.cs" */
            {
                cs = ragelmachine_start;
                ts = -1;
                te = -1;
                act = 0;
            }

            /* #line 44 "rt.rl" */

            // exec

            /* #line 69 "rt.cs" */
            {
                sbyte _klen;
                sbyte _trans;
                int _acts;
                int _nacts;
                sbyte _keys;

                if (p == pe)
                    goto _test_eof;
                if (cs == 0)
                    goto _out;
                _resume:
                _acts = _ragelmachine_from_state_actions[cs];
                _nacts = _ragelmachine_actions[_acts++];
                while (_nacts-- > 0)
                {
                    switch (_ragelmachine_actions[_acts++])
                    {
                        case 1:
                            /* #line 1 "NONE" */
                            {
                                ts = p;
                            }
                            break;
                            /* #line 88 "rt.cs" */
                        default:
                            break;
                    }
                }

                _keys = _ragelmachine_key_offsets[cs];
                _trans = (sbyte) _ragelmachine_index_offsets[cs];

                _klen = _ragelmachine_single_lengths[cs];
                if (_klen > 0)
                {
                    sbyte _lower = _keys;
                    sbyte _mid;
                    sbyte _upper = (sbyte) (_keys + _klen - 1);
                    while (true)
                    {
                        if (_upper < _lower)
                            break;

                        _mid = (sbyte) (_lower + ((_upper - _lower) >> 1));
                        if (data[p] < _ragelmachine_trans_keys[_mid])
                            _upper = (sbyte) (_mid - 1);
                        else if (data[p] > _ragelmachine_trans_keys[_mid])
                            _lower = (sbyte) (_mid + 1);
                        else
                        {
                            _trans += (sbyte) (_mid - _keys);
                            goto _match;
                        }
                    }
                    _keys += (sbyte) _klen;
                    _trans += (sbyte) _klen;
                }

                _klen = _ragelmachine_range_lengths[cs];
                if (_klen > 0)
                {
                    sbyte _lower = _keys;
                    sbyte _mid;
                    sbyte _upper = (sbyte) (_keys + (_klen << 1) - 2);
                    while (true)
                    {
                        if (_upper < _lower)
                            break;

                        _mid = (sbyte) (_lower + (((_upper - _lower) >> 1) & ~1));
                        if (data[p] < _ragelmachine_trans_keys[_mid])
                            _upper = (sbyte) (_mid - 2);
                        else if (data[p] > _ragelmachine_trans_keys[_mid + 1])
                            _lower = (sbyte) (_mid + 2);
                        else
                        {
                            _trans += (sbyte) ((_mid - _keys) >> 1);
                            goto _match;
                        }
                    }
                    _trans += (sbyte) _klen;
                }

                _match:
                _eof_trans:
                cs = _ragelmachine_trans_targs[_trans];

                if (_ragelmachine_trans_actions[_trans] == 0)
                    goto _again;

                _acts = _ragelmachine_trans_actions[_trans];
                _nacts = _ragelmachine_actions[_acts++];
                while (_nacts-- > 0)
                {
                    switch (_ragelmachine_actions[_acts++])
                    {
                        case 2:
                            /* #line 7 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new ATTRIBUTE_SEGMENT_IDENTIFIER(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 3:
                            /* #line 8 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new TOKEN_SEGMENT_IDENTIFIER(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 4:
                            /* #line 9 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new PRODUCTION_SEGMENT_IDENTIFIER(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 5:
                            /* #line 10 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new SEMICOLON(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 6:
                            /* #line 4 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new EQUALS(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 7:
                            /* #line 6 "rt.rl" */
                            {
                                te = p + 1;
                                {
                                    yield return new PROD_EQUALS(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                        case 8:
                            /* #line 22 "rt.rl" */
                            {
                                te = p + 1;
                            }
                            break;
                        case 9:
                            /* #line 5 "rt.rl" */
                            {
                                te = p;
                                p--;
                                {
                                    yield return new ID(data.Substring(ts, te - ts));
                                }
                            }
                            break;
                            /* #line 177 "rt.cs" */
                        default:
                            break;
                    }
                }

                _again:
                _acts = _ragelmachine_to_state_actions[cs];
                _nacts = _ragelmachine_actions[_acts++];
                while (_nacts-- > 0)
                {
                    switch (_ragelmachine_actions[_acts++])
                    {
                        case 0:
                            /* #line 1 "NONE" */
                            {
                                ts = -1;
                            }
                            break;
                            /* #line 189 "rt.cs" */
                        default:
                            break;
                    }
                }

                if (cs == 0)
                    goto _out;
                if (++p != pe)
                    goto _resume;
                _test_eof:
                {
                }
                if (p == eof)
                {
                    if (_ragelmachine_eof_trans[cs] > 0)
                    {
                        _trans = (sbyte) (_ragelmachine_eof_trans[cs] - 1);
                        goto _eof_trans;
                    }
                }

                _out:
                {
                }
            }

            /* #line 47 "rt.rl" */
            if (p == eof)
            {
                yield return new EOF("$end");
            }
            else
            {
                throw new Exception("Lexing error - Unrecognized input.");
            }
        }
    }
}
