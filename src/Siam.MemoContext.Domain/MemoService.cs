﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Siam.MemoContext.Domain
{
    public class MemoService : DomainService, IMemoService
    {
        public async Task<Result> CreateMemos(int memoCount)
        {
            var rnd = new Random();

            for (var i = 0; i < memoCount; i++)
            {
                await ExecuteAsync(new UpdateMemo(MemoId.New, 
                    new MemoDocument(
                        Guid.NewGuid().ToString(),
                        new DocumentNumber(rnd.Next(1000).ToString()),
                        new Address("127000", "Россия", "Москва",
                            "проспект Мира",rnd.Next(1000).ToString()
                        ))));
            }
            return Result.Success;
        }
    }
}