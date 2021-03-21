﻿using System;
using System.Collections.Generic;
using System.Text;

using Key = System.UInt64;
using Bitboard = System.UInt64;
using Move = System.Int32;
using File = System.Int32;
using Rank = System.Int32;
using Score = System.Int32;
using Square = System.Int32;
using Color = System.Int32;
using Value = System.Int32;
using PieceType = System.Int32;
using Piece = System.Int32;
using CastleRight = System.Int32;
using Depth = System.Int32;
using Result = System.Int32;
using ScaleFactor = System.Int32;
using Phase = System.Int32;
using TracedType = System.Int32;
using NodeType = System.Int32;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Portfish
{
    /// A Polyglot book is a series of "entries" of 16 bytes. All integers are
    /// stored highest byte first (regardless of size). The entries are ordered
    /// according to key. Lowest key first.
    internal struct BookEntry
    {
        internal UInt64 key;
        internal UInt16 move;
        internal UInt16 count;
        internal UInt32 learn;
    };

    internal static class Book
    {
        #region PolyGlotRandoms

        // Random numbers from PolyGlot, used to compute book hash keys
        static readonly Key[] PolyGlotRandoms = new Key[781] {
                0x9D39247E33776D41UL, 0x2AF7398005AAA5C7UL, 0x44DB015024623547UL,
                0x9C15F73E62A76AE2UL, 0x75834465489C0C89UL, 0x3290AC3A203001BFUL,
                0x0FBBAD1F61042279UL, 0xE83A908FF2FB60CAUL, 0x0D7E765D58755C10UL,
                0x1A083822CEAFE02DUL, 0x9605D5F0E25EC3B0UL, 0xD021FF5CD13A2ED5UL,
                0x40BDF15D4A672E32UL, 0x011355146FD56395UL, 0x5DB4832046F3D9E5UL,
                0x239F8B2D7FF719CCUL, 0x05D1A1AE85B49AA1UL, 0x679F848F6E8FC971UL,
                0x7449BBFF801FED0BUL, 0x7D11CDB1C3B7ADF0UL, 0x82C7709E781EB7CCUL,
                0xF3218F1C9510786CUL, 0x331478F3AF51BBE6UL, 0x4BB38DE5E7219443UL,
                0xAA649C6EBCFD50FCUL, 0x8DBD98A352AFD40BUL, 0x87D2074B81D79217UL,
                0x19F3C751D3E92AE1UL, 0xB4AB30F062B19ABFUL, 0x7B0500AC42047AC4UL,
                0xC9452CA81A09D85DUL, 0x24AA6C514DA27500UL, 0x4C9F34427501B447UL,
                0x14A68FD73C910841UL, 0xA71B9B83461CBD93UL, 0x03488B95B0F1850FUL,
                0x637B2B34FF93C040UL, 0x09D1BC9A3DD90A94UL, 0x3575668334A1DD3BUL,
                0x735E2B97A4C45A23UL, 0x18727070F1BD400BUL, 0x1FCBACD259BF02E7UL,
                0xD310A7C2CE9B6555UL, 0xBF983FE0FE5D8244UL, 0x9F74D14F7454A824UL,
                0x51EBDC4AB9BA3035UL, 0x5C82C505DB9AB0FAUL, 0xFCF7FE8A3430B241UL,
                0x3253A729B9BA3DDEUL, 0x8C74C368081B3075UL, 0xB9BC6C87167C33E7UL,
                0x7EF48F2B83024E20UL, 0x11D505D4C351BD7FUL, 0x6568FCA92C76A243UL,
                0x4DE0B0F40F32A7B8UL, 0x96D693460CC37E5DUL, 0x42E240CB63689F2FUL,
                0x6D2BDCDAE2919661UL, 0x42880B0236E4D951UL, 0x5F0F4A5898171BB6UL,
                0x39F890F579F92F88UL, 0x93C5B5F47356388BUL, 0x63DC359D8D231B78UL,
                0xEC16CA8AEA98AD76UL, 0x5355F900C2A82DC7UL, 0x07FB9F855A997142UL,
                0x5093417AA8A7ED5EUL, 0x7BCBC38DA25A7F3CUL, 0x19FC8A768CF4B6D4UL,
                0x637A7780DECFC0D9UL, 0x8249A47AEE0E41F7UL, 0x79AD695501E7D1E8UL,
                0x14ACBAF4777D5776UL, 0xF145B6BECCDEA195UL, 0xDABF2AC8201752FCUL,
                0x24C3C94DF9C8D3F6UL, 0xBB6E2924F03912EAUL, 0x0CE26C0B95C980D9UL,
                0xA49CD132BFBF7CC4UL, 0xE99D662AF4243939UL, 0x27E6AD7891165C3FUL,
                0x8535F040B9744FF1UL, 0x54B3F4FA5F40D873UL, 0x72B12C32127FED2BUL,
                0xEE954D3C7B411F47UL, 0x9A85AC909A24EAA1UL, 0x70AC4CD9F04F21F5UL,
                0xF9B89D3E99A075C2UL, 0x87B3E2B2B5C907B1UL, 0xA366E5B8C54F48B8UL,
                0xAE4A9346CC3F7CF2UL, 0x1920C04D47267BBDUL, 0x87BF02C6B49E2AE9UL,
                0x092237AC237F3859UL, 0xFF07F64EF8ED14D0UL, 0x8DE8DCA9F03CC54EUL,
                0x9C1633264DB49C89UL, 0xB3F22C3D0B0B38EDUL, 0x390E5FB44D01144BUL,
                0x5BFEA5B4712768E9UL, 0x1E1032911FA78984UL, 0x9A74ACB964E78CB3UL,
                0x4F80F7A035DAFB04UL, 0x6304D09A0B3738C4UL, 0x2171E64683023A08UL,
                0x5B9B63EB9CEFF80CUL, 0x506AACF489889342UL, 0x1881AFC9A3A701D6UL,
                0x6503080440750644UL, 0xDFD395339CDBF4A7UL, 0xEF927DBCF00C20F2UL,
                0x7B32F7D1E03680ECUL, 0xB9FD7620E7316243UL, 0x05A7E8A57DB91B77UL,
                0xB5889C6E15630A75UL, 0x4A750A09CE9573F7UL, 0xCF464CEC899A2F8AUL,
                0xF538639CE705B824UL, 0x3C79A0FF5580EF7FUL, 0xEDE6C87F8477609DUL,
                0x799E81F05BC93F31UL, 0x86536B8CF3428A8CUL, 0x97D7374C60087B73UL,
                0xA246637CFF328532UL, 0x043FCAE60CC0EBA0UL, 0x920E449535DD359EUL,
                0x70EB093B15B290CCUL, 0x73A1921916591CBDUL, 0x56436C9FE1A1AA8DUL,
                0xEFAC4B70633B8F81UL, 0xBB215798D45DF7AFUL, 0x45F20042F24F1768UL,
                0x930F80F4E8EB7462UL, 0xFF6712FFCFD75EA1UL, 0xAE623FD67468AA70UL,
                0xDD2C5BC84BC8D8FCUL, 0x7EED120D54CF2DD9UL, 0x22FE545401165F1CUL,
                0xC91800E98FB99929UL, 0x808BD68E6AC10365UL, 0xDEC468145B7605F6UL,
                0x1BEDE3A3AEF53302UL, 0x43539603D6C55602UL, 0xAA969B5C691CCB7AUL,
                0xA87832D392EFEE56UL, 0x65942C7B3C7E11AEUL, 0xDED2D633CAD004F6UL,
                0x21F08570F420E565UL, 0xB415938D7DA94E3CUL, 0x91B859E59ECB6350UL,
                0x10CFF333E0ED804AUL, 0x28AED140BE0BB7DDUL, 0xC5CC1D89724FA456UL,
                0x5648F680F11A2741UL, 0x2D255069F0B7DAB3UL, 0x9BC5A38EF729ABD4UL,
                0xEF2F054308F6A2BCUL, 0xAF2042F5CC5C2858UL, 0x480412BAB7F5BE2AUL,
                0xAEF3AF4A563DFE43UL, 0x19AFE59AE451497FUL, 0x52593803DFF1E840UL,
                0xF4F076E65F2CE6F0UL, 0x11379625747D5AF3UL, 0xBCE5D2248682C115UL,
                0x9DA4243DE836994FUL, 0x066F70B33FE09017UL, 0x4DC4DE189B671A1CUL,
                0x51039AB7712457C3UL, 0xC07A3F80C31FB4B4UL, 0xB46EE9C5E64A6E7CUL,
                0xB3819A42ABE61C87UL, 0x21A007933A522A20UL, 0x2DF16F761598AA4FUL,
                0x763C4A1371B368FDUL, 0xF793C46702E086A0UL, 0xD7288E012AEB8D31UL,
                0xDE336A2A4BC1C44BUL, 0x0BF692B38D079F23UL, 0x2C604A7A177326B3UL,
                0x4850E73E03EB6064UL, 0xCFC447F1E53C8E1BUL, 0xB05CA3F564268D99UL,
                0x9AE182C8BC9474E8UL, 0xA4FC4BD4FC5558CAUL, 0xE755178D58FC4E76UL,
                0x69B97DB1A4C03DFEUL, 0xF9B5B7C4ACC67C96UL, 0xFC6A82D64B8655FBUL,
                0x9C684CB6C4D24417UL, 0x8EC97D2917456ED0UL, 0x6703DF9D2924E97EUL,
                0xC547F57E42A7444EUL, 0x78E37644E7CAD29EUL, 0xFE9A44E9362F05FAUL,
                0x08BD35CC38336615UL, 0x9315E5EB3A129ACEUL, 0x94061B871E04DF75UL,
                0xDF1D9F9D784BA010UL, 0x3BBA57B68871B59DUL, 0xD2B7ADEEDED1F73FUL,
                0xF7A255D83BC373F8UL, 0xD7F4F2448C0CEB81UL, 0xD95BE88CD210FFA7UL,
                0x336F52F8FF4728E7UL, 0xA74049DAC312AC71UL, 0xA2F61BB6E437FDB5UL,
                0x4F2A5CB07F6A35B3UL, 0x87D380BDA5BF7859UL, 0x16B9F7E06C453A21UL,
                0x7BA2484C8A0FD54EUL, 0xF3A678CAD9A2E38CUL, 0x39B0BF7DDE437BA2UL,
                0xFCAF55C1BF8A4424UL, 0x18FCF680573FA594UL, 0x4C0563B89F495AC3UL,
                0x40E087931A00930DUL, 0x8CFFA9412EB642C1UL, 0x68CA39053261169FUL,
                0x7A1EE967D27579E2UL, 0x9D1D60E5076F5B6FUL, 0x3810E399B6F65BA2UL,
                0x32095B6D4AB5F9B1UL, 0x35CAB62109DD038AUL, 0xA90B24499FCFAFB1UL,
                0x77A225A07CC2C6BDUL, 0x513E5E634C70E331UL, 0x4361C0CA3F692F12UL,
                0xD941ACA44B20A45BUL, 0x528F7C8602C5807BUL, 0x52AB92BEB9613989UL,
                0x9D1DFA2EFC557F73UL, 0x722FF175F572C348UL, 0x1D1260A51107FE97UL,
                0x7A249A57EC0C9BA2UL, 0x04208FE9E8F7F2D6UL, 0x5A110C6058B920A0UL,
                0x0CD9A497658A5698UL, 0x56FD23C8F9715A4CUL, 0x284C847B9D887AAEUL,
                0x04FEABFBBDB619CBUL, 0x742E1E651C60BA83UL, 0x9A9632E65904AD3CUL,
                0x881B82A13B51B9E2UL, 0x506E6744CD974924UL, 0xB0183DB56FFC6A79UL,
                0x0ED9B915C66ED37EUL, 0x5E11E86D5873D484UL, 0xF678647E3519AC6EUL,
                0x1B85D488D0F20CC5UL, 0xDAB9FE6525D89021UL, 0x0D151D86ADB73615UL,
                0xA865A54EDCC0F019UL, 0x93C42566AEF98FFBUL, 0x99E7AFEABE000731UL,
                0x48CBFF086DDF285AUL, 0x7F9B6AF1EBF78BAFUL, 0x58627E1A149BBA21UL,
                0x2CD16E2ABD791E33UL, 0xD363EFF5F0977996UL, 0x0CE2A38C344A6EEDUL,
                0x1A804AADB9CFA741UL, 0x907F30421D78C5DEUL, 0x501F65EDB3034D07UL,
                0x37624AE5A48FA6E9UL, 0x957BAF61700CFF4EUL, 0x3A6C27934E31188AUL,
                0xD49503536ABCA345UL, 0x088E049589C432E0UL, 0xF943AEE7FEBF21B8UL,
                0x6C3B8E3E336139D3UL, 0x364F6FFA464EE52EUL, 0xD60F6DCEDC314222UL,
                0x56963B0DCA418FC0UL, 0x16F50EDF91E513AFUL, 0xEF1955914B609F93UL,
                0x565601C0364E3228UL, 0xECB53939887E8175UL, 0xBAC7A9A18531294BUL,
                0xB344C470397BBA52UL, 0x65D34954DAF3CEBDUL, 0xB4B81B3FA97511E2UL,
                0xB422061193D6F6A7UL, 0x071582401C38434DUL, 0x7A13F18BBEDC4FF5UL,
                0xBC4097B116C524D2UL, 0x59B97885E2F2EA28UL, 0x99170A5DC3115544UL,
                0x6F423357E7C6A9F9UL, 0x325928EE6E6F8794UL, 0xD0E4366228B03343UL,
                0x565C31F7DE89EA27UL, 0x30F5611484119414UL, 0xD873DB391292ED4FUL,
                0x7BD94E1D8E17DEBCUL, 0xC7D9F16864A76E94UL, 0x947AE053EE56E63CUL,
                0xC8C93882F9475F5FUL, 0x3A9BF55BA91F81CAUL, 0xD9A11FBB3D9808E4UL,
                0x0FD22063EDC29FCAUL, 0xB3F256D8ACA0B0B9UL, 0xB03031A8B4516E84UL,
                0x35DD37D5871448AFUL, 0xE9F6082B05542E4EUL, 0xEBFAFA33D7254B59UL,
                0x9255ABB50D532280UL, 0xB9AB4CE57F2D34F3UL, 0x693501D628297551UL,
                0xC62C58F97DD949BFUL, 0xCD454F8F19C5126AUL, 0xBBE83F4ECC2BDECBUL,
                0xDC842B7E2819E230UL, 0xBA89142E007503B8UL, 0xA3BC941D0A5061CBUL,
                0xE9F6760E32CD8021UL, 0x09C7E552BC76492FUL, 0x852F54934DA55CC9UL,
                0x8107FCCF064FCF56UL, 0x098954D51FFF6580UL, 0x23B70EDB1955C4BFUL,
                0xC330DE426430F69DUL, 0x4715ED43E8A45C0AUL, 0xA8D7E4DAB780A08DUL,
                0x0572B974F03CE0BBUL, 0xB57D2E985E1419C7UL, 0xE8D9ECBE2CF3D73FUL,
                0x2FE4B17170E59750UL, 0x11317BA87905E790UL, 0x7FBF21EC8A1F45ECUL,
                0x1725CABFCB045B00UL, 0x964E915CD5E2B207UL, 0x3E2B8BCBF016D66DUL,
                0xBE7444E39328A0ACUL, 0xF85B2B4FBCDE44B7UL, 0x49353FEA39BA63B1UL,
                0x1DD01AAFCD53486AUL, 0x1FCA8A92FD719F85UL, 0xFC7C95D827357AFAUL,
                0x18A6A990C8B35EBDUL, 0xCCCB7005C6B9C28DUL, 0x3BDBB92C43B17F26UL,
                0xAA70B5B4F89695A2UL, 0xE94C39A54A98307FUL, 0xB7A0B174CFF6F36EUL,
                0xD4DBA84729AF48ADUL, 0x2E18BC1AD9704A68UL, 0x2DE0966DAF2F8B1CUL,
                0xB9C11D5B1E43A07EUL, 0x64972D68DEE33360UL, 0x94628D38D0C20584UL,
                0xDBC0D2B6AB90A559UL, 0xD2733C4335C6A72FUL, 0x7E75D99D94A70F4DUL,
                0x6CED1983376FA72BUL, 0x97FCAACBF030BC24UL, 0x7B77497B32503B12UL,
                0x8547EDDFB81CCB94UL, 0x79999CDFF70902CBUL, 0xCFFE1939438E9B24UL,
                0x829626E3892D95D7UL, 0x92FAE24291F2B3F1UL, 0x63E22C147B9C3403UL,
                0xC678B6D860284A1CUL, 0x5873888850659AE7UL, 0x0981DCD296A8736DUL,
                0x9F65789A6509A440UL, 0x9FF38FED72E9052FUL, 0xE479EE5B9930578CUL,
                0xE7F28ECD2D49EECDUL, 0x56C074A581EA17FEUL, 0x5544F7D774B14AEFUL,
                0x7B3F0195FC6F290FUL, 0x12153635B2C0CF57UL, 0x7F5126DBBA5E0CA7UL,
                0x7A76956C3EAFB413UL, 0x3D5774A11D31AB39UL, 0x8A1B083821F40CB4UL,
                0x7B4A38E32537DF62UL, 0x950113646D1D6E03UL, 0x4DA8979A0041E8A9UL,
                0x3BC36E078F7515D7UL, 0x5D0A12F27AD310D1UL, 0x7F9D1A2E1EBE1327UL,
                0xDA3A361B1C5157B1UL, 0xDCDD7D20903D0C25UL, 0x36833336D068F707UL,
                0xCE68341F79893389UL, 0xAB9090168DD05F34UL, 0x43954B3252DC25E5UL,
                0xB438C2B67F98E5E9UL, 0x10DCD78E3851A492UL, 0xDBC27AB5447822BFUL,
                0x9B3CDB65F82CA382UL, 0xB67B7896167B4C84UL, 0xBFCED1B0048EAC50UL,
                0xA9119B60369FFEBDUL, 0x1FFF7AC80904BF45UL, 0xAC12FB171817EEE7UL,
                0xAF08DA9177DDA93DUL, 0x1B0CAB936E65C744UL, 0xB559EB1D04E5E932UL,
                0xC37B45B3F8D6F2BAUL, 0xC3A9DC228CAAC9E9UL, 0xF3B8B6675A6507FFUL,
                0x9FC477DE4ED681DAUL, 0x67378D8ECCEF96CBUL, 0x6DD856D94D259236UL,
                0xA319CE15B0B4DB31UL, 0x073973751F12DD5EUL, 0x8A8E849EB32781A5UL,
                0xE1925C71285279F5UL, 0x74C04BF1790C0EFEUL, 0x4DDA48153C94938AUL,
                0x9D266D6A1CC0542CUL, 0x7440FB816508C4FEUL, 0x13328503DF48229FUL,
                0xD6BF7BAEE43CAC40UL, 0x4838D65F6EF6748FUL, 0x1E152328F3318DEAUL,
                0x8F8419A348F296BFUL, 0x72C8834A5957B511UL, 0xD7A023A73260B45CUL,
                0x94EBC8ABCFB56DAEUL, 0x9FC10D0F989993E0UL, 0xDE68A2355B93CAE6UL,
                0xA44CFE79AE538BBEUL, 0x9D1D84FCCE371425UL, 0x51D2B1AB2DDFB636UL,
                0x2FD7E4B9E72CD38CUL, 0x65CA5B96B7552210UL, 0xDD69A0D8AB3B546DUL,
                0x604D51B25FBF70E2UL, 0x73AA8A564FB7AC9EUL, 0x1A8C1E992B941148UL,
                0xAAC40A2703D9BEA0UL, 0x764DBEAE7FA4F3A6UL, 0x1E99B96E70A9BE8BUL,
                0x2C5E9DEB57EF4743UL, 0x3A938FEE32D29981UL, 0x26E6DB8FFDF5ADFEUL,
                0x469356C504EC9F9DUL, 0xC8763C5B08D1908CUL, 0x3F6C6AF859D80055UL,
                0x7F7CC39420A3A545UL, 0x9BFB227EBDF4C5CEUL, 0x89039D79D6FC5C5CUL,
                0x8FE88B57305E2AB6UL, 0xA09E8C8C35AB96DEUL, 0xFA7E393983325753UL,
                0xD6B6D0ECC617C699UL, 0xDFEA21EA9E7557E3UL, 0xB67C1FA481680AF8UL,
                0xCA1E3785A9E724E5UL, 0x1CFC8BED0D681639UL, 0xD18D8549D140CAEAUL,
                0x4ED0FE7E9DC91335UL, 0xE4DBF0634473F5D2UL, 0x1761F93A44D5AEFEUL,
                0x53898E4C3910DA55UL, 0x734DE8181F6EC39AUL, 0x2680B122BAA28D97UL,
                0x298AF231C85BAFABUL, 0x7983EED3740847D5UL, 0x66C1A2A1A60CD889UL,
                0x9E17E49642A3E4C1UL, 0xEDB454E7BADC0805UL, 0x50B704CAB602C329UL,
                0x4CC317FB9CDDD023UL, 0x66B4835D9EAFEA22UL, 0x219B97E26FFC81BDUL,
                0x261E4E4C0A333A9DUL, 0x1FE2CCA76517DB90UL, 0xD7504DFA8816EDBBUL,
                0xB9571FA04DC089C8UL, 0x1DDC0325259B27DEUL, 0xCF3F4688801EB9AAUL,
                0xF4F5D05C10CAB243UL, 0x38B6525C21A42B0EUL, 0x36F60E2BA4FA6800UL,
                0xEB3593803173E0CEUL, 0x9C4CD6257C5A3603UL, 0xAF0C317D32ADAA8AUL,
                0x258E5A80C7204C4BUL, 0x8B889D624D44885DUL, 0xF4D14597E660F855UL,
                0xD4347F66EC8941C3UL, 0xE699ED85B0DFB40DUL, 0x2472F6207C2D0484UL,
                0xC2A1E7B5B459AEB5UL, 0xAB4F6451CC1D45ECUL, 0x63767572AE3D6174UL,
                0xA59E0BD101731A28UL, 0x116D0016CB948F09UL, 0x2CF9C8CA052F6E9FUL,
                0x0B090A7560A968E3UL, 0xABEEDDB2DDE06FF1UL, 0x58EFC10B06A2068DUL,
                0xC6E57A78FBD986E0UL, 0x2EAB8CA63CE802D7UL, 0x14A195640116F336UL,
                0x7C0828DD624EC390UL, 0xD74BBE77E6116AC7UL, 0x804456AF10F5FB53UL,
                0xEBE9EA2ADF4321C7UL, 0x03219A39EE587A30UL, 0x49787FEF17AF9924UL,
                0xA1E9300CD8520548UL, 0x5B45E522E4B1B4EFUL, 0xB49C3B3995091A36UL,
                0xD4490AD526F14431UL, 0x12A8F216AF9418C2UL, 0x001F837CC7350524UL,
                0x1877B51E57A764D5UL, 0xA2853B80F17F58EEUL, 0x993E1DE72D36D310UL,
                0xB3598080CE64A656UL, 0x252F59CF0D9F04BBUL, 0xD23C8E176D113600UL,
                0x1BDA0492E7E4586EUL, 0x21E0BD5026C619BFUL, 0x3B097ADAF088F94EUL,
                0x8D14DEDB30BE846EUL, 0xF95CFFA23AF5F6F4UL, 0x3871700761B3F743UL,
                0xCA672B91E9E4FA16UL, 0x64C8E531BFF53B55UL, 0x241260ED4AD1E87DUL,
                0x106C09B972D2E822UL, 0x7FBA195410E5CA30UL, 0x7884D9BC6CB569D8UL,
                0x0647DFEDCD894A29UL, 0x63573FF03E224774UL, 0x4FC8E9560F91B123UL,
                0x1DB956E450275779UL, 0xB8D91274B9E9D4FBUL, 0xA2EBEE47E2FBFCE1UL,
                0xD9F1F30CCD97FB09UL, 0xEFED53D75FD64E6BUL, 0x2E6D02C36017F67FUL,
                0xA9AA4D20DB084E9BUL, 0xB64BE8D8B25396C1UL, 0x70CB6AF7C2D5BCF0UL,
                0x98F076A4F7A2322EUL, 0xBF84470805E69B5FUL, 0x94C3251F06F90CF3UL,
                0x3E003E616A6591E9UL, 0xB925A6CD0421AFF3UL, 0x61BDD1307C66E300UL,
                0xBF8D5108E27E0D48UL, 0x240AB57A8B888B20UL, 0xFC87614BAF287E07UL,
                0xEF02CDD06FFDB432UL, 0xA1082C0466DF6C0AUL, 0x8215E577001332C8UL,
                0xD39BB9C3A48DB6CFUL, 0x2738259634305C14UL, 0x61CF4F94C97DF93DUL,
                0x1B6BACA2AE4E125BUL, 0x758F450C88572E0BUL, 0x959F587D507A8359UL,
                0xB063E962E045F54DUL, 0x60E8ED72C0DFF5D1UL, 0x7B64978555326F9FUL,
                0xFD080D236DA814BAUL, 0x8C90FD9B083F4558UL, 0x106F72FE81E2C590UL,
                0x7976033A39F7D952UL, 0xA4EC0132764CA04BUL, 0x733EA705FAE4FA77UL,
                0xB4D8F77BC3E56167UL, 0x9E21F4F903B33FD9UL, 0x9D765E419FB69F6DUL,
                0xD30C088BA61EA5EFUL, 0x5D94337FBFAF7F5BUL, 0x1A4E4822EB4D7A59UL,
                0x6FFE73E81B637FB3UL, 0xDDF957BC36D8B9CAUL, 0x64D0E29EEA8838B3UL,
                0x08DD9BDFD96B9F63UL, 0x087E79E5A57D1D13UL, 0xE328E230E3E2B3FBUL,
                0x1C2559E30F0946BEUL, 0x720BF5F26F4D2EAAUL, 0xB0774D261CC609DBUL,
                0x443F64EC5A371195UL, 0x4112CF68649A260EUL, 0xD813F2FAB7F5C5CAUL,
                0x660D3257380841EEUL, 0x59AC2C7873F910A3UL, 0xE846963877671A17UL,
                0x93B633ABFA3469F8UL, 0xC0C0F5A60EF4CDCFUL, 0xCAF21ECD4377B28CUL,
                0x57277707199B8175UL, 0x506C11B9D90E8B1DUL, 0xD83CC2687A19255FUL,
                0x4A29C6465A314CD1UL, 0xED2DF21216235097UL, 0xB5635C95FF7296E2UL,
                0x22AF003AB672E811UL, 0x52E762596BF68235UL, 0x9AEBA33AC6ECC6B0UL,
                0x944F6DE09134DFB6UL, 0x6C47BEC883A7DE39UL, 0x6AD047C430A12104UL,
                0xA5B1CFDBA0AB4067UL, 0x7C45D833AFF07862UL, 0x5092EF950A16DA0BUL,
                0x9338E69C052B8E7BUL, 0x455A4B4CFE30E3F5UL, 0x6B02E63195AD0CF8UL,
                0x6B17B224BAD6BF27UL, 0xD1E0CCD25BB9C169UL, 0xDE0C89A556B9AE70UL,
                0x50065E535A213CF6UL, 0x9C1169FA2777B874UL, 0x78EDEFD694AF1EEDUL,
                0x6DC93D9526A50E68UL, 0xEE97F453F06791EDUL, 0x32AB0EDB696703D3UL,
                0x3A6853C7E70757A7UL, 0x31865CED6120F37DUL, 0x67FEF95D92607890UL,
                0x1F2B1D1F15F6DC9CUL, 0xB69E38A8965C6B65UL, 0xAA9119FF184CCCF4UL,
                0xF43C732873F24C13UL, 0xFB4A3D794A9A80D2UL, 0x3550C2321FD6109CUL,
                0x371F77E76BB8417EUL, 0x6BFA9AAE5EC05779UL, 0xCD04F3FF001A4778UL,
                0xE3273522064480CAUL, 0x9F91508BFFCFC14AUL, 0x049A7F41061A9E60UL,
                0xFCB6BE43A9F2FE9BUL, 0x08DE8A1C7797DA9BUL, 0x8F9887E6078735A1UL,
                0xB5B4071DBFC73A66UL, 0x230E343DFBA08D33UL, 0x43ED7F5A0FAE657DUL,
                0x3A88A0FBBCB05C63UL, 0x21874B8B4D2DBC4FUL, 0x1BDEA12E35F6A8C9UL,
                0x53C065C6C8E63528UL, 0xE34A1D250E7A8D6BUL, 0xD6B04D3B7651DD7EUL,
                0x5E90277E7CB39E2DUL, 0x2C046F22062DC67DUL, 0xB10BB459132D0A26UL,
                0x3FA9DDFB67E2F199UL, 0x0E09B88E1914F7AFUL, 0x10E8B35AF3EEAB37UL,
                0x9EEDECA8E272B933UL, 0xD4C718BC4AE8AE5FUL, 0x81536D601170FC20UL,
                0x91B534F885818A06UL, 0xEC8177F83F900978UL, 0x190E714FADA5156EUL,
                0xB592BF39B0364963UL, 0x89C350C893AE7DC1UL, 0xAC042E70F8B383F2UL,
                0xB49B52E587A1EE60UL, 0xFB152FE3FF26DA89UL, 0x3E666E6F69AE2C15UL,
                0x3B544EBE544C19F9UL, 0xE805A1E290CF2456UL, 0x24B33C9D7ED25117UL,
                0xE74733427B72F0C1UL, 0x0A804D18B7097475UL, 0x57E3306D881EDB4FUL,
                0x4AE7D6A36EB5DBCBUL, 0x2D8D5432157064C8UL, 0xD1E649DE1E7F268BUL,
                0x8A328A1CEDFE552CUL, 0x07A3AEC79624C7DAUL, 0x84547DDC3E203C94UL,
                0x990A98FD5071D263UL, 0x1A4FF12616EEFC89UL, 0xF6F7FD1431714200UL,
                0x30C05B1BA332F41CUL, 0x8D2636B81555A786UL, 0x46C9FEB55D120902UL,
                0xCCEC0A73B49C9921UL, 0x4E9D2827355FC492UL, 0x19EBB029435DCB0FUL,
                0x4659D2B743848A2CUL, 0x963EF2C96B33BE31UL, 0x74F85198B05A2E7DUL,
                0x5A0F544DD2B1FB18UL, 0x03727073C2E134B1UL, 0xC7F6AA2DE59AEA61UL,
                0x352787BAA0D7C22FUL, 0x9853EAB63B5E0B35UL, 0xABBDCDD7ED5C0860UL,
                0xCF05DAF5AC8D77B0UL, 0x49CAD48CEBF4A71EUL, 0x7A4C10EC2158C4A6UL,
                0xD9E92AA246BF719EUL, 0x13AE978D09FE5557UL, 0x730499AF921549FFUL,
                0x4E4B705B92903BA4UL, 0xFF577222C14F0A3AUL, 0x55B6344CF97AAFAEUL,
                0xB862225B055B6960UL, 0xCAC09AFBDDD2CDB4UL, 0xDAF8E9829FE96B5FUL,
                0xB5FDFC5D3132C498UL, 0x310CB380DB6F7503UL, 0xE87FBB46217A360EUL,
                0x2102AE466EBB1148UL, 0xF8549E1A3AA5E00DUL, 0x07A69AFDCC42261AUL,
                0xC4C118BFE78FEAAEUL, 0xF9F4892ED96BD438UL, 0x1AF3DBE25D8F45DAUL,
                0xF5B4B0B0D2DEEEB4UL, 0x962ACEEFA82E1C84UL, 0x046E3ECAAF453CE9UL,
                0xF05D129681949A4CUL, 0x964781CE734B3C84UL, 0x9C2ED44081CE5FBDUL,
                0x522E23F3925E319EUL, 0x177E00F9FC32F791UL, 0x2BC60A63A6F3B3F2UL,
                0x222BBFAE61725606UL, 0x486289DDCC3D6780UL, 0x7DC7785B8EFDFC80UL,
                0x8AF38731C02BA980UL, 0x1FAB64EA29A2DDF7UL, 0xE4D9429322CD065AUL,
                0x9DA058C67844F20CUL, 0x24C0E332B70019B0UL, 0x233003B5A6CFE6ADUL,
                0xD586BD01C5C217F6UL, 0x5E5637885F29BC2BUL, 0x7EBA726D8C94094BUL,
                0x0A56A5F0BFE39272UL, 0xD79476A84EE20D06UL, 0x9E4C1269BAA4BF37UL,
                0x17EFEE45B0DEE640UL, 0x1D95B0A5FCF90BC6UL, 0x93CBE0B699C2585DUL,
                0x65FA4F227A2B6D79UL, 0xD5F9E858292504D5UL, 0xC2B5A03F71471A6FUL,
                0x59300222B4561E00UL, 0xCE2F8642CA0712DCUL, 0x7CA9723FBB2E8988UL,
                0x2785338347F2BA08UL, 0xC61BB3A141E50E8CUL, 0x150F361DAB9DEC26UL,
                0x9F6A419D382595F4UL, 0x64A53DC924FE7AC9UL, 0x142DE49FFF7A7C3DUL,
                0x0C335248857FA9E7UL, 0x0A9C32D5EAE45305UL, 0xE6C42178C4BBB92EUL,
                0x71F1CE2490D20B07UL, 0xF1BCC3D275AFE51AUL, 0xE728E8C83C334074UL,
                0x96FBF83A12884624UL, 0x81A1549FD6573DA5UL, 0x5FA7867CAF35E149UL,
                0x56986E2EF3ED091BUL, 0x917F1DD5F8886C61UL, 0xD20D8C88C8FFE65FUL,
                0x31D71DCE64B2C310UL, 0xF165B587DF898190UL, 0xA57E6339DD2CF3A0UL,
                0x1EF6E6DBB1961EC9UL, 0x70CC73D90BC26E24UL, 0xE21A6B35DF0C3AD7UL,
                0x003A93D8B2806962UL, 0x1C99DED33CB890A1UL, 0xCF3145DE0ADD4289UL,
                0xD0E4427A5514FB72UL, 0x77C621CC9FB3A483UL, 0x67A34DAC4356550BUL,
                0xF8D626AAAF278509UL
                };

        #endregion

        // Offsets to the PolyGlotRandoms[] array of zobrist keys
        private const int ZobPieceOffset = 0;
        private const int ZobCastleOffset = 768;
        private const int ZobEnPassantOffset = 772;
        private const int ZobTurnOffset = 780;

        private const long SIZE_OF_BOOKENTRY = 16;

        private static readonly RKISS RKiss = new RKISS();

#if PORTABLE
        private static bool bookNotExists = false;
#endif

        internal static void init()
        {
            for (long i = Math.Abs(DateTime.Now.Millisecond % 10000L); i > 0; i--)
            {
                RKiss.rand(); // Make random number generation less deterministic
            }
        }

        /// Book::probe() tries to find a book move for the given position. If no move
        /// is found returns MOVE_NONE. If pickBest is true returns always the highest
        /// rated move, otherwise randomly chooses one, based on the move score.
        internal static Move probe(Position pos, string filename, bool pickBest)
        {
#if PORTABLE

            #region DLL book

            if (bookNotExists) { return MoveC.MOVE_NONE; }

            BookEntry e = new BookEntry();
            UInt16 best = 0;
            uint sum = 0;
            Move move = MoveC.MOVE_NONE;
            UInt64 key = book_key(pos);

            try
            {
                System.Reflection.Assembly bookAssembly = System.Reflection.Assembly.Load("PortfishBook");
                using (Stream fs = bookAssembly.GetManifestResourceStream("PortfishBook.book.bin"))
                {
                    UInt64 size = (UInt64)(fs.Length / SIZE_OF_BOOKENTRY);
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        binary_search(key, size, br);
                        while (Read(ref e, br) && (e.key == key))
                        {
                            best = Math.Max(best, e.count);
                            sum += e.count;

                            // Choose book move according to its score. If a move has a very
                            // high score it has higher probability to be choosen than a move
                            // with lower score. Note that first entry is always chosen.
                            if ((RKiss.rand() % sum < e.count)
                                || (pickBest && e.count == best))
                            {
                                move = e.move;
                            }
                        }
                    }
                }
            }
            catch(System.IO.FileNotFoundException)
            {
                bookNotExists = true;
                return MoveC.MOVE_NONE;
            }

            #endregion

#else

            #region File system read

            if (!System.IO.File.Exists(filename)) return MoveC.MOVE_NONE;

            BookEntry e = new BookEntry();
            UInt16 best = 0;
            uint sum = 0;
            Move move = MoveC.MOVE_NONE;
            UInt64 key = book_key(pos);

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                UInt64 size = (UInt64)(fs.Length / SIZE_OF_BOOKENTRY);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    binary_search(key, size, br);
                    while (Read(ref e, br) && (e.key == key))
                    {
                        best = Math.Max(best, e.count);
                        sum += e.count;

                        // Choose book move according to its score. If a move has a very
                        // high score it has higher probability to be choosen than a move
                        // with lower score. Note that first entry is always chosen.
                        if (((sum!=0) && (RKiss.rand() % sum < e.count))
                            || (pickBest && e.count == best))
                        {
                            move = e.move;
                        }
                    }
                    br.Close();
                }
                fs.Close();
            }

            #endregion

#endif

            if (move != 0)
            {
                // A PolyGlot book move is encoded as follows:
                //
                // bit  0- 5: destination square (from 0 to 63)
                // bit  6-11: origin square (from 0 to 63)
                // bit 12-14: promotion piece (from KNIGHT == 1 to QUEEN == 4)
                //
                // Castling moves follow "king captures rook" representation. So in case book
                // move is a promotion we have to convert to our representation, in all the
                // other cases we can directly compare with a Move after having masked out
                // the special Move's flags (bit 14-15) that are not supported by PolyGlot.
                int pt = (move >> 12) & 7;
                if (pt != 0)
                {
                    move = Utils.make_promotion(Utils.from_sq(move), Utils.to_sq(move), (pt + 1));
                }

                // Add 'special move' flags and verify it is legal
                MList mlist = MListBroker.GetObject(); mlist.pos = 0;
                Movegen.generate_legal(pos, mlist.moves, ref mlist.pos);
                for (int i = 0; i < mlist.pos; i++)
                {
                    if (move == (mlist.moves[i].move & 0x3FFF))
                    {
                        Move retval = mlist.moves[i].move;
                        MListBroker.Free();
                        return retval;
                    }
                }
                MListBroker.Free();
            }

            return MoveC.MOVE_NONE;
        }

        private static bool Read(ref BookEntry e, BinaryReader br)
        {
            if (br.BaseStream.Length == br.BaseStream.Position) return false;

            byte[] t = br.ReadBytes((int)SIZE_OF_BOOKENTRY);

            e.key =
                (((UInt64)t[0]) << 56) |
                (((UInt64)t[1]) << 48) |
                (((UInt64)t[2]) << 40) |
                (((UInt64)t[3]) << 32) |
                (((UInt64)t[4]) << 24) |
                (((UInt64)t[5]) << 16) |
                (((UInt64)t[6]) << 8) |
                (((UInt64)t[7]) << 0);

            e.move = (UInt16)(
                (((UInt64)t[8]) << 8) |
                (((UInt64)t[9]) << 0));

            e.count = (UInt16)(
                (((UInt64)t[10]) << 8) |
                (((UInt64)t[11]) << 0));

            e.learn = (UInt32)(
                (((UInt64)t[12]) << 24) |
                (((UInt64)t[13]) << 16) |
                (((UInt64)t[14]) << 8) |
                (((UInt64)t[15]) << 0));

            return true;
        }

        // book_key() returns the PolyGlot hash key of the given position
        private static UInt64 book_key(Position pos)
        {
            UInt64 key = 0;
            Bitboard b = pos.occupied_squares;

            while (b != 0)
            {
                // Piece offset is at 64 * polyPiece where polyPiece is defined as:
                // BP = 0, WP = 1, BN = 2, WN = 3, ... BK = 10, WK = 11
                Square s = Utils.pop_1st_bit(ref b);
                Piece p = pos.piece_on(s);
                int polyPiece = 2 * (Utils.type_of(p) - 1) + (Utils.color_of(p) == ColorC.WHITE ? 1 : 0);
                key ^= PolyGlotRandoms[ZobPieceOffset + (64 * polyPiece + s)];
            }

            b = (ulong)pos.can_castle_CR(CastleRightC.ALL_CASTLES);

            while (b != 0)
                key ^= PolyGlotRandoms[ZobCastleOffset + Utils.pop_1st_bit(ref b)];

            if (pos.st.epSquare != SquareC.SQ_NONE)
                key ^= PolyGlotRandoms[ZobEnPassantOffset + Utils.file_of(pos.st.epSquare)];

            if (pos.sideToMove == ColorC.WHITE)
                key ^= PolyGlotRandoms[ZobTurnOffset + 0];

            return key;
        }

        /// Book::binary_search() takes a book key as input, and does a binary search
        /// through the book file for the given key. File stream current position is set
        /// to the leftmost book entry with the same key as the input.
        private static void binary_search(UInt64 key, UInt64 size, BinaryReader br)
        {
            UInt64 low, high, mid;
            BookEntry e = new BookEntry();

            low = 0;
            high = (ulong)(size - 1);

            Debug.Assert(low <= high);

            while (low < high)
            {
                mid = (low + high) / 2;

                Debug.Assert(mid >= low && mid < high);

                br.BaseStream.Seek((long)(mid * SIZE_OF_BOOKENTRY), SeekOrigin.Begin);
                Read(ref e, br);

                if (key <= e.key)
                    high = mid;
                else
                    low = mid + 1;
            }

            Debug.Assert(low == high);

            br.BaseStream.Seek((long)(low * SIZE_OF_BOOKENTRY), SeekOrigin.Begin);
        }
    }
}
