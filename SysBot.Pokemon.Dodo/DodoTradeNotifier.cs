﻿using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;

namespace SysBot.Pokemon.Dodo
{
    public class DodoTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }

        private string ChannelId { get; }

        public DodoTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string channelId)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            ChannelId = channelId;
            LogUtil.LogText($"创建交易细节: {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>> OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"交换已取消, 取消原因:{msg}";
            LogUtil.LogText(line);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID,line, ChannelId);
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"{info.Trainer.TrainerName}: " + (tradedToUser != 0
                ? $"交易完成。享受您的{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}({(Species) tradedToUser})!"
                : "交易完成!");
            var text =
                 $"我收到精灵的种类:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}\n" +
                 $"PID:{result.PID:X}\n" +
                 $"加密常数:{result.EncryptionConstant:X}\n" +
                 $"训练家姓名:{result.OT_Name}\n" +
                 $"训练家性别:{(result.OT_Gender == 0 ? "男" : "女")}\n" +
                 $"训练家表ID:{result.TrainerID7}\n" +
                 $"训练家里ID:{result.TrainerSID7}"; 
            LogUtil.LogText(message);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, message, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(),text);
             
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"正在初始化与{info.Trainer.TrainerName}(ID: {info.ID})的交易{receive}";
            msg += $" 交易密码为: {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            var text = $"队列号:***{info.ID}***\n正在派送:***{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}***\n密码:见私信\n状态:初始化\n请准备好\n";
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(),
                $"正在派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n您的密码:{info.Code:0000 0000}\n{routine.InGameName}正在派送");
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"正在等待{name}!,机器人IGN为{routine.InGameName}.";
            message += $" 交易密码为: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            var text = $"我正在等你,{name},第{info.ID}号\n我的游戏ID为{routine.InGameName}\n正在派送:***{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}***\n密码:***见私信***\n状态:搜索中\n";
            DodoBot<T>.SendChannelMessage(text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"我正在等你,{name}\n密码:{info.Code:0000 0000}\n请速来领取");
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"{result.FileName}的详细信息: " + message;
            LogUtil.LogText(msg);
            String IVstring = "";
            if (result.IV_ATK == 0)
                IVstring = "(0攻)";
            if (result.IV_SPE == 0)
                IVstring += "(0速)";
            if (result.IVs[0] + result.IVs[1] + result.IVs[2] + result.IVs[3] + result.IVs[4] + result.IVs[5] == 186)
                IVstring = "(6V)";
            if (result.Species != 0 && info.Type == PokeTradeType.Dump)
            {
                var text = message +
                    $"宝可梦:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}\n" +
                    $"个体值:{result.IV_HP},{result.IV_ATK},{result.IV_DEF},{result.IV_SPA},{result.IV_SPD},{result.IV_SPE}" + IVstring + "\n"+
                    $"闪光:{(result.IsShiny ? "闪了闪了闪了闪了闪了闪了" : "否")}";
                DodoBot<T>.SendChannelMessage(text, ChannelId);
            }
           
        }
    }
}