﻿using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB.Engine;

namespace LiteDB
{
    internal partial class SqlParser
    {
        /// <summary>
        /// UPDATE - update documents - if used with {key} = {exprValue} will merge current document with this fields
        ///          if used with { key: value } will replace current document with new document
        ///  UPDATE {collection}
        ///     SET [{key} = {exprValue}, {key} = {exprValue} | { newDoc }]
        /// [ WHERE {whereExpr} ]
        /// </summary>
        private BsonDataReader ParseUpadate()
        {
            var collection = _tokenizer.ReadToken().Expect(TokenType.Word).Value;
            _tokenizer.ReadToken().Expect("SET");

            var transform = BsonExpression.Create(_tokenizer, _parameters, BsonExpressionParserMode.UpdateDocument);

            // optional where
            BsonExpression where = null;
            var token = _tokenizer.LookAhead();

            if (token.Is("WHERE"))
            {
                // read WHERE
                _tokenizer.ReadToken();

                where = BsonExpression.Create(_tokenizer, _parameters, BsonExpressionParserMode.Full);
            }

            // read eof
            _tokenizer.ReadToken().Expect(TokenType.EOF, TokenType.SemiColon);

            var result = _engine.UpdateMany(collection, transform, where);

            return new BsonDataReader(result);
        }
    }
}